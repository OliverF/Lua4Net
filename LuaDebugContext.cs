namespace Lua4Net
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Lua4Net.Native;
    using Lua4Net.Types;

    public class LuaDebugContext
    {
        private readonly Lua lua;

        private NativeLuaDebug luaDebug;

        internal LuaDebugContext(Lua lua, NativeLuaDebug luaDebug)
        {
            this.lua = lua;
            this.luaDebug = luaDebug;
        }

        public int Line
        {
            get
            {
                return this.luaDebug.Currentline;
            }
        }

        public IList<LuaDebugLocalVariable> FetchLocalVariables(bool filterTemporaryVariables)
        {
            using (this.lua.Stack.CreateCountChecker(0))
            {
                var variableList = new List<LuaDebugLocalVariable>();

                int i = 1;
                IntPtr variableNamePtr;

                while ((variableNamePtr = NativeMethods.Lua_getlocal(this.lua.LuaState, ref this.luaDebug, i)) != IntPtr.Zero)
                {
                    var variableValue = this.lua.Stack.Pop();
                    string variableName = NativeMethods.StringPointerToString(variableNamePtr);

                    bool add = true;

                    if (filterTemporaryVariables && variableName.Equals("(*temporary)"))
                    {
                        add = false;
                    }

                    if (add)
                    {
                        var localVariable = new LuaDebugLocalVariable(i - 1, variableName, variableValue);
                        variableList.Add(localVariable);
                    }

                    i++;
                }

                return variableList;
            }
        }

        public IDictionary<LuaValueType, LuaType> FetchLocalVariableTableFields(int index)
        {
            return this.FetchLocalVariableTableFields(index, LuaTablePath.Empty);
        }

        public IDictionary<LuaValueType, LuaType> FetchLocalVariableTableFields(int index, IEnumerable<LuaValueType> path)
        {
            using (this.lua.Stack.CreateCountChecker(0))
            {
                int indexOneBased = checked(index + 1);

                if (NativeMethods.Lua_getlocal(this.lua.LuaState, ref this.luaDebug, indexOneBased) == IntPtr.Zero)
                {
                    throw new LuaException("lua get local returned null: invalid index");
                }

                IDictionary<LuaValueType, LuaType> dictionary;
                try
                {
                    var topAsTable = this.lua.Stack.Top as LuaTable;
                    if (topAsTable == null)
                    {
                        throw new LuaException(string.Format(CultureInfo.InvariantCulture, "Local variable at index {0} is no table.", index));
                    }

                    dictionary = topAsTable.FetchTableFields(path);
                }
                finally
                {
                    this.lua.Stack.PopAndForgetEntries(1);
                }

                return dictionary;
            }
        }
    }
}