namespace Lua4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Lua4Net.Native;
    using Lua4Net.Types;

    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "It IS a stack.")]
    public class LuaVirtualStack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaVirtualStack"/> class.
        /// </summary>
        /// <param name="lua">
        /// A lua reference.
        /// </param>
        internal LuaVirtualStack(Lua lua)
        {
            this.Lua = lua;
        }

        /// <summary>
        /// Gets the stack size (stack top value).
        /// </summary>
        public int Count
        {
            get
            {
                return NativeMethods.Lua_gettop(this.Lua.LuaState);
            }
        }

        /// <summary>
        /// Gets (and encapsulates) the top stack entry.
        /// </summary>
        public LuaType Top
        {
            get
            {
                return this.GetEntryAtIndex(-1);
            }
        }

        /// <summary>
        /// Gets or sets the lua reference.
        /// </summary>
        private Lua Lua { get; set; }

        public LuaType GetEntryAtIndex(int index)
        {
            LuaType result = null;

            // adjust negative index
            if (index < 0)
            {
                index = this.Count + index + 1;
            }

            Trace.Assert(this.IsValidStackIndex(index), "this.IsValidStackIndex(index)");

            var type = NativeMethods.Lua_type(this.Lua.LuaState, index);

            switch (type)
            {
                case NativeLuaTypes.Nil:
                    result = new LuaNilValue();
                    break;
                case NativeLuaTypes.Boolean:
                    result = new LuaBoolean(NativeMethods.Lua_toboolean(this.Lua.LuaState, index));
                    break;
                case NativeLuaTypes.Number:
                    result = new LuaNumber(NativeMethods.Lua_tonumber(this.Lua.LuaState, index));
                    break;
                case NativeLuaTypes.String:
                    IntPtr len;
                    var strPtr = NativeMethods.Lua_tolstring(this.Lua.LuaState, index, out len);
                    result = new LuaString(NativeMethods.StringPointerToString(strPtr, len.ToInt32()));
                    break;
                case NativeLuaTypes.Table:
                    result = new LuaTable(this.Lua, index, true);
                    break;
                case NativeLuaTypes.Function:
                    result = new LuaFunction(this.Lua, index);
                    break;
            }

            Trace.Assert(result != null, "result should be assigned at this point");

            return result;
        }

        /// <summary>
        /// Determines whether the specified stack index is a nil value.
        /// </summary>
        /// <param name="index">
        /// A stack index.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified stack index is a nil value; 
        ///   otherwise, <c>false</c> (even if it is an invalid stack index).
        /// </returns>
        public bool IsNilValueAtIndex(int index)
        {
            return NativeMethods.Lua_type(this.Lua.LuaState, index) == NativeLuaTypes.Nil;
        }

        /// <summary>
        /// Determines whether the specified stack index is a <i>Valid</i> stack index.
        /// </summary>
        /// <param name="index">
        /// A stack index.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified stack index is a <i>Valid</i> stack index; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidStackIndex(int index)
        {
            return 0 < index && index <= this.Count;
        }

        /// <summary>
        /// Pop one entry from stack.
        /// </summary>
        /// <returns>
        /// The top stack entry.
        /// </returns>
        public LuaType Pop()
        {
            var result = this.Top;
            NativeMethods.Lua_pop(this.Lua.LuaState, 1);

            return result;
        }

        /// <summary>
        /// Pop one entry and assign it to a global variable.
        /// </summary>
        /// <param name="name">
        /// Global variable name.
        /// </param>
        public void PopAndSetGlobal(string name)
        {
            NativeMethods.Lua_setglobal(this.Lua.LuaState, name);
        }

        /// <summary>
        /// Pop entries from the stack and throw them away.
        /// </summary>
        /// <param name="count">
        /// Number of items to pop.
        /// </param>
        public void PopAndForgetEntries(int count)
        {
            if (count > 0)
            {
                NativeMethods.Lua_pop(this.Lua.LuaState, count);
            }
        }

        /// <summary>
        /// Push a lua value type onto the stack.
        /// </summary>
        /// <param name="value">
        /// A lua value type.
        /// </param>
        public void PushValue(LuaValueType value)
        {
            var valueBoolean = value as LuaBoolean;
            var valueNilValue = value as LuaNilValue;
            var valueNumber = value as LuaNumber;
            var valueString = value as LuaString;

            if (valueBoolean != null)
            {
                NativeMethods.Lua_pushboolean(this.Lua.LuaState, valueBoolean.Value);
            }
            else if (valueNilValue != null)
            {
                NativeMethods.Lua_pushnil(this.Lua.LuaState);
            }
            else if (valueNumber != null)
            {
                NativeMethods.Lua_pushnumber(this.Lua.LuaState, valueNumber.Value);
            }
            else if (valueString != null)
            {
                NativeMethods.Lua_pushlstring(this.Lua.LuaState, valueString.Value, new IntPtr(valueString.Value.Length));
            }
            else
            {
                throw new LuaException("invalid lua value type");
            }
        }

        public int PushCFunctionOutputList(IList<LuaValueType> outputList)
        {
            foreach (var item in outputList)
            {
                this.PushValue(item);
            }

            return outputList.Count;
        }

        public IList<LuaType> FetchCFunctionInputList()
        {
            var inputList = new List<LuaType>();

            for (int i = 1; i <= this.Count; i++)
            {
                inputList.Add(this.GetEntryAtIndex(i));
            }

            return inputList;
        }

        public IDisposable CreateCountChecker(int expectedRelativeStackCount)
        {
            var checker = new CountChecker(this, this.Count + expectedRelativeStackCount);

            return checker;
        }

        /// <summary>
        /// Push a c function delegate onto the stack.
        /// </summary>
        /// <param name="function">
        /// C function delegate.
        /// </param>
        internal void PushCFunction(NativeMethods.LuaCFunctionDelegate function)
        {
            NativeMethods.Lua_pushcclosure(this.Lua.LuaState, function, 0);
        }

        private class CountChecker : IDisposable
        {
            private readonly LuaVirtualStack luaVirtualStack;

            private readonly int expectedStackCount;

            public CountChecker(LuaVirtualStack luaVirtualStack, int expectedStackCount)
            {
                this.luaVirtualStack = luaVirtualStack;
                this.expectedStackCount = expectedStackCount;
            }

            public void Dispose()
            {
#if DEBUG
                var actualStackCount = this.luaVirtualStack.Count;

                var msg = string.Format(CultureInfo.InvariantCulture, "Expected stack count: {0}, actual: {1}", this.expectedStackCount, actualStackCount);
                Debug.Assert(actualStackCount == this.expectedStackCount, msg);
#endif
            }
        }
    }
}