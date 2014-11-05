namespace Lua4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    using Lua4Net.Native;
    using Lua4Net.Types;

    /// <summary>
    /// Lua main class.
    /// </summary>
    public class Lua : IDisposable
    {
        private const string DefaultChunkName = "code_chunk";

        /// <summary>
        /// A list of all managed global functions.
        /// </summary>
        private readonly List<LuaManagedFunction> managedGlobalFunctions = new List<LuaManagedFunction>();

        /// <summary>
        /// A list of all registered managed objects. Needed to hold references (prevent GC freeing).
        /// </summary>
        private readonly List<LuaManagedObject> registeredManagedObjects = new List<LuaManagedObject>();

        private readonly NativeMethods.LuaCFunctionDelegate luaAtPanicHandlerDelegate;

        private bool disposed;

        private NativeMethods.LuaHookFunctionDelegate hookHandlerDelegate;

        private LuaHookType hookType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lua"/> class.
        /// </summary>
        public Lua()
        {
            this.LuaState = NativeMethods.LuaL_newstate();
            NativeMethods.luaL_openlibs(LuaState);
            Trace.Assert(this.LuaState != IntPtr.Zero, "this.LuaState != IntPtr.Zero");

            // the following line is really, really important, because if we wouldn't keep a reference to the delegate object here,
            // the garbage collector would free the delegate object => lua would call the freed delegate (see "CallbackOnCollectedDelegate MDA")
            this.luaAtPanicHandlerDelegate = this.LuaAtPanicHandler;

            NativeMethods.Lua_atpanic(this.LuaState, this.luaAtPanicHandlerDelegate);

            this.LoadedStandardLibraries = new HashSet<LuaStandardLibrary>();
            this.Stack = new LuaVirtualStack(this);

            this.Global = new LuaGlobalTable(this);
        }

        ~Lua()
        {
            this.Dispose(false);
        }

        public event EventHandler<LuaPrintFunctionOutputEventArgs> PrintFunctionOutput;

        public event EventHandler<LuaLineHookEventArgs> LineHook;

        public LuaGlobalTable Global { get; private set; }

        /// <summary>
        /// Gets the list of already loaded standard libraries.
        /// </summary>
        public HashSet<LuaStandardLibrary> LoadedStandardLibraries { get; private set; }

        /// <summary>
        /// Gets or sets the hook type.
        /// </summary>
        public LuaHookType HookType
        {
            get
            {
                return this.hookType;
            }

            set
            {
                NativeLuaHookEventMasks mask;

                switch (value)
                {
                    case LuaHookType.None:
                        mask = 0;
                        break;

                    case LuaHookType.LineHook:
                        mask = NativeLuaHookEventMasks.LuaMaskline;
                        break;

                    default:
                        throw new LuaException("invalid hook type");
                }

                // the following line is really, really important, because if we wouldn't keep a reference to the delegate object here,
                // the garbage collector would free the delegate object => lua would call the freed delegate (see "CallbackOnCollectedDelegate MDA")
                this.hookHandlerDelegate = this.HookHandler;

                var setHookRes = NativeMethods.Lua_sethook(this.LuaState, this.hookHandlerDelegate, mask, 0);
                Trace.Assert(setHookRes == 1, "setHookRes == 1");

                this.hookType = value;
            }
        }

        /// <summary>
        /// Gets the lua virtual stack reference.
        /// </summary>
        public LuaVirtualStack Stack { get; private set; }

        /// <summary>
        /// Gets the lua state pointer.
        /// </summary>
        internal IntPtr LuaState { get; private set; }

        /// <summary>
        /// Loads (opens) a standard library.
        /// </summary>
        /// <param name="luaStandardLibrary">
        /// A standard library.
        /// </param>
        public void LoadStandardLibrary(LuaStandardLibrary luaStandardLibrary)
        {
            using (this.Stack.CreateCountChecker(0))
            {
                int openRet;

                switch (luaStandardLibrary)
                {
                    case LuaStandardLibrary.Base:
                        openRet = NativeMethods.Luaopen_base(this.LuaState);
                        break;

                    case LuaStandardLibrary.String:
                        openRet = NativeMethods.Luaopen_string(this.LuaState);
                        break;

                    case LuaStandardLibrary.Table:
                        openRet = NativeMethods.Luaopen_table(this.LuaState);
                        break;

                    case LuaStandardLibrary.Math:
                        openRet = NativeMethods.Luaopen_math(this.LuaState);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("luaStandardLibrary");
                }

                this.Stack.PopAndForgetEntries(openRet);

                this.LoadedStandardLibraries.Add(luaStandardLibrary);
            }
        }

        /// <summary>
        /// Executes the specified code chunk.
        /// </summary>
        /// <param name="lines">
        /// The code chunk lines.
        /// </param>
        /// <returns>
        /// An array representing the return values.
        /// </returns>
        public LuaType[] Execute(IEnumerable<string> lines)
        {
            return this.Execute(lines, DefaultChunkName);
        }

        /// <summary>
        /// Executes the specified code chunk.
        /// </summary>
        /// <param name="lines">
        /// The code chunk lines.
        /// </param>
        /// <param name="chunkName">
        /// The code chunk name.
        /// </param>
        /// <returns>
        /// An array representing the return values.
        /// </returns>
        public LuaType[] Execute(IEnumerable<string> lines, string chunkName)
        {
            var code = new StringBuilder();
            bool firstLine = true;

            foreach (var line in lines)
            {
                if (firstLine)
                {
                    code.Append(line);
                    firstLine = false;
                }
                else
                {
                    code.AppendLine().Append(line);
                }
            }

            return this.Execute(code.ToString(), chunkName);
        }

        /// <summary>
        /// Executes the specified code chunk.
        /// </summary>
        /// <param name="code">
        /// The code chunk.
        /// </param>
        /// <returns>
        /// An array representing the return values.
        /// </returns>
        public LuaType[] Execute(string code)
        {
            return this.Execute(code, DefaultChunkName);
        }

        /// <summary>
        /// Executes the specified code chunk.
        /// </summary>
        /// <param name="code">
        /// The code chunk.
        /// </param>
        /// <param name="chunkName">
        /// The code chunk name.
        /// </param>
        /// <returns>
        /// An array representing the return values.
        /// </returns>
        public LuaType[] Execute(string code, string chunkName)
        {
            this.CheckIfDisposed();

            // reset stack
            this.Stack.PopAndForgetEntries(this.Stack.Count);

            var oldStackCount = this.Stack.Count;

            var loadRet = NativeMethods.LuaL_loadbuffer(this.LuaState, code, (IntPtr)code.Length, chunkName);

            if (loadRet > 0)
            {
                // error occured ... error message on stack
                Debug.Assert(this.Stack.Count == oldStackCount + 1, "stack count");

                // error value is a string
                var topAsString = (LuaString)this.Stack.Pop();

                throw ParseErrorMessageForSourceCodeLineAndCreateException(topAsString.Value, false);
            }

            Debug.Assert(this.Stack.Count == oldStackCount + 1, "stack count");
            Debug.Assert(this.Stack.Top is LuaFunction, "stack top");

            int stackCountBeforeReturnValues = this.Stack.Count - 1; // - 1 because of the function entry

            // pcall: fetches function and runs it
            int callRet = NativeMethods.Lua_pcall(
                this.LuaState, 
                0, // no input args
                NativeMethods.LuaMultret, // push all return values onto the stack
                0); // no error function

            if (callRet > 0)
            {
                // error occured ... error message on stack
                Debug.Assert(this.Stack.Count == oldStackCount + 1, "stack count");

                // error value is a string
                var topAsString = (LuaString)this.Stack.Pop();

                throw ParseErrorMessageForSourceCodeLineAndCreateException(topAsString.Value, true);
            }

            // no error ... fetch return values
            var ret = new List<LuaType>();

            for (int i = this.Stack.Count; i > stackCountBeforeReturnValues; i--)
            {
                ret.Insert(0, this.Stack.Pop());
            }

            Debug.Assert(this.Stack.Count == oldStackCount, "stack count");

            return ret.ToArray();
        }

        public void RegisterGlobalFunction(string name, LuaManagedFunctionHandler managedFunctionHandler)
        {
            using (this.Stack.CreateCountChecker(0))
            {
                var managedFunction = new LuaManagedFunction(managedFunctionHandler, this.RegisteredFunctionGetLuaReferenceByLuaState);

                managedFunction.PushFunctionToStack(this.Stack);
                this.Stack.PopAndSetGlobal(name);

                // the following is really, really important, because if we wouldn't keep a reference to the RegisteredFunction object here,
                // the garbage collector would free the delegate object => lua would call the freed delegate (see "CallbackOnCollectedDelegate MDA")
                this.managedGlobalFunctions.Add(managedFunction);
            }
        }

        public void OverrideGlobalPrintFunction()
        {
            if (!this.LoadedStandardLibraries.Contains(LuaStandardLibrary.Base))
            {
                throw new LuaException("please load base library before overriding the print function");
            }

            this.RegisterGlobalFunction("print", this.PrintFunctionOverrideHandler);
        }

        public void RegisterGlobalManagedObject(string name, LuaManagedObject managedObject)
        {
            managedObject.CreateMetatableAndPushToStack(this);

            this.Stack.PopAndSetGlobal(name);

            if (!this.registeredManagedObjects.Contains(managedObject))
            {
                this.registeredManagedObjects.Add(managedObject);
            }
        }

        #region Dispose Stuff

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            ////Trace.TraceInformation("Lua.Dispose(" + disposing + ")");

            if (!this.disposed)
            {
                if (disposing)
                {
                    // dispose managed resources
                }

                // remove the references for every managed object
                for (int i = this.registeredManagedObjects.Count - 1; i >= 0; i--)
                {
                    this.registeredManagedObjects[i].RemoveLuaReference(this);
                    this.registeredManagedObjects.RemoveAt(i);
                }

                // cleanup unmanaged lua state handle
                if (this.LuaState != IntPtr.Zero)
                {
                    NativeMethods.Lua_close(this.LuaState);
                    this.LuaState = IntPtr.Zero;
                }

                this.disposed = true;
            }
        }

        #endregion

        private static LuaException ParseErrorMessageForSourceCodeLineAndCreateException(string errorMessage, bool runtimeException)
        {
            var regex = new Regex("(.*):(\\d+):\\s(.*)");
            var match = regex.Match(errorMessage);

            if (!match.Success)
            {
                if (runtimeException)
                {
                    return new LuaRuntimeErrorException(errorMessage);
                }

                return new LuaSyntaxErrorException(errorMessage);
            }
            else
            {
                string chunkName = match.Groups[1].Value;
                var line = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                if (runtimeException)
                {
                    return new LuaRuntimeErrorException(chunkName, line, match.Groups[3].Value);
                }

                return new LuaSyntaxErrorException(chunkName, line, match.Groups[3].Value);
            }
        }

        private Lua RegisteredFunctionGetLuaReferenceByLuaState(IntPtr luaState)
        {
            Trace.Assert(luaState == this.LuaState, "asserting luaState == this.LuaState");

            return this;
        }

        private void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Lua");
            }
        }

        private int LuaAtPanicHandler(IntPtr luaState)
        {
            Debug.Assert(luaState == this.LuaState, "lua reference");

            // error value is a string
            var topAsString = (LuaString)this.Stack.Pop();

            throw new LuaErrorOutsideProtectedEnvironmentException(topAsString.Value);
        }

        private void PrintFunctionOverrideHandler(LuaManagedFunctionArgs args)
        {
            string text = string.Empty;

            using (this.Stack.CreateCountChecker(0))
            {
                for (int i = 0; i < args.Input.Count; i++)
                {
                    NativeMethods.Lua_getglobal(this.LuaState, @"tostring");
                    NativeMethods.Lua_pushvalue(this.LuaState, this.Stack.Count - args.Input.Count + i);

                    NativeMethods.Lua_call(this.LuaState, 1, 1); // stack: -2 +1

                    string str = ((LuaString)this.Stack.Pop()).Value;

                    if (i == 0)
                    {
                        text = str;
                    }
                    else
                    {
                        text += "\t" + str;
                    }
                }
            }

            if (this.PrintFunctionOutput != null)
            {
                this.PrintFunctionOutput(this, new LuaPrintFunctionOutputEventArgs(text));
            }
        }

        private void HookHandler(IntPtr lua, ref NativeLuaDebug luaDebug)
        {
            Trace.Assert(lua == this.LuaState, "lua reference");

            var debugContext = new LuaDebugContext(this, luaDebug);

            ////var getInfoRet = NativeMethods.Lua_getinfo(lua, "nSl", ref luaDebug);
            ////Trace.Assert(getInfoRet != 0, "hook get info result != 0");
            if (this.LineHook != null)
            {
                this.LineHook(this, new LuaLineHookEventArgs(debugContext));
            }
        }
    }
}