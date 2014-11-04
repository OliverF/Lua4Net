namespace Lua4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Lua4Net.Native;
    using Lua4Net.Types;

    /// <summary>
    /// Represents a managed Lua function.
    /// </summary>
    public class LuaManagedFunction
    {
        private readonly LuaManagedFunctionHandler managedFunctionHandler;

        private readonly Func<IntPtr, Lua> getLuaReferenceByLuaState;

        private readonly NativeMethods.LuaCFunctionDelegate luaCFunctionHandlerDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaManagedFunction"/> class.
        /// </summary>
        /// <param name="managedFunctionHandler">
        /// The managed function handler.
        /// </param>
        /// <param name="getLuaReferenceByLuaState">
        /// A function used to get a Lua instance from a Lua state.
        /// </param>
        public LuaManagedFunction(LuaManagedFunctionHandler managedFunctionHandler, Func<IntPtr, Lua> getLuaReferenceByLuaState)
        {
            if (managedFunctionHandler == null)
            {
                throw new ArgumentNullException("managedFunctionHandler");
            }

            Debug.Assert(getLuaReferenceByLuaState != null, "asserting getLuaReferenceByLuaState != null");

            this.managedFunctionHandler = managedFunctionHandler;
            this.getLuaReferenceByLuaState = getLuaReferenceByLuaState;

            this.luaCFunctionHandlerDelegate = this.LuaCFunctionHandler;
        }

        /// <summary>
        /// Creates and returns a managed function argument exception.
        /// </summary>
        /// <param name="argNumber">
        /// The argument number.
        /// </param>
        /// <param name="functionName">
        /// Name of the function.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// A managed function argument exception.
        /// </returns>
        public static LuaManagedFunctionException CreateArgumentException(int argNumber, string functionName, string message)
        {
            var msg = string.Format(CultureInfo.InvariantCulture, "bad argument #{0} to '{1}' ({2})", argNumber, functionName, message);

            return new LuaManagedFunctionException(msg);
        }

        /// <summary>
        /// Pushes the registered function as Lua function.
        /// </summary>
        /// <param name="stack">
        /// The Lua stack.
        /// </param>
        public void PushFunctionToStack(LuaVirtualStack stack)
        {
            stack.PushCFunction(this.luaCFunctionHandlerDelegate);
        }

        /// <summary>
        /// C function handler of the registered lua function.
        /// </summary>
        /// <param name="luaState">
        /// Lua state pointer.
        /// </param>
        /// <returns>
        /// Number of return values.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Lua4Net.Native.NativeMethods.Lua_error(System.IntPtr)", Justification = "Lua_error doesn't return")]
        private int LuaCFunctionHandler(IntPtr luaState)
        {
            var lua = this.getLuaReferenceByLuaState(luaState);

            var inputList = lua.Stack.FetchCFunctionInputList();
            var outputList = new List<LuaValueType>();

            var args = new LuaManagedFunctionArgs(lua, inputList, outputList);

            var stackCountBeforeCall = lua.Stack.Count;

            try
            {
                this.managedFunctionHandler(args);
            }
            catch (LuaManagedFunctionException ex)
            {
                // generate lua error (lua_error doesn't return)
                lua.Stack.PushValue(new LuaString(ex.Message));
                NativeMethods.Lua_error(lua.LuaState);
            }

            Debug.Assert(lua.Stack.Count == stackCountBeforeCall, "stack count");

            return lua.Stack.PushCFunctionOutputList(outputList);
        }
    }
}