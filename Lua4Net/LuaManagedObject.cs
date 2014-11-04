namespace Lua4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Lua4Net.Native;
    using Lua4Net.Types;

    /// <summary>
    /// Abstract base class for managed Lua objects.
    /// </summary>
    public abstract class LuaManagedObject
    {
        /// <summary>
        /// The Lua references dictionary.
        ///   This dictionary will be used to get the corresponding <see cref="Lua"/>-object of a Lua state variable.
        /// </summary>
        private readonly IDictionary<IntPtr, Lua> luaReferences;

        private readonly NativeMethods.LuaCFunctionDelegate indexCFunctionHandlerDelegate;

        private readonly NativeMethods.LuaCFunctionDelegate newindexCFunctionHandlerDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaManagedObject"/> class.
        /// </summary>
        protected LuaManagedObject()
        {
            this.luaReferences = new Dictionary<IntPtr, Lua>();

            this.indexCFunctionHandlerDelegate = this.IndexCFunctionHandler;
            this.newindexCFunctionHandlerDelegate = this.NewIndexCFunctionHandler;
        }

        /// <summary>
        /// Gets the current Lua references count.
        /// </summary>
        /// <value>
        /// The current Lua references count.
        /// </value>
        protected int LuaReferencesCount
        {
            get
            {
                lock (this.luaReferences)
                {
                    return this.luaReferences.Count;
                }
            }
        }

        /// <summary>
        /// Creates a metatable, assigns all the metamethods, and pushed it onto the stack.
        /// </summary>
        /// <param name="lua">
        /// A lua reference.
        /// </param>
        public void CreateMetatableAndPushToStack(Lua lua)
        {
            using (lua.Stack.CreateCountChecker(+1))
            {
                this.AddLuaReference(lua);

                // create a table for the managed object
                NativeMethods.Lua_createtable(lua.LuaState, 0, 0);

                // create the metatable
                NativeMethods.Lua_createtable(lua.LuaState, 0, 0);

                lua.Stack.PushCFunction(this.indexCFunctionHandlerDelegate);
                NativeMethods.Lua_setfield(lua.LuaState, -2, "__index");

                lua.Stack.PushCFunction(this.newindexCFunctionHandlerDelegate);
                NativeMethods.Lua_setfield(lua.LuaState, -2, "__newindex");

                // assign the metatable
                NativeMethods.Lua_setmetatable(lua.LuaState, -2);
            }
        }

        /// <summary>
        /// Removes a Lua reference.
        /// </summary>
        /// <param name="lua">
        /// A Lua reference.
        /// </param>
        protected internal virtual void RemoveLuaReference(Lua lua)
        {
            lock (this.luaReferences)
            {
                this.luaReferences.Remove(lua.LuaState);
            }
        }

        /// <summary>
        /// Adds a Lua reference.
        /// </summary>
        /// <param name="lua">
        /// A Lua reference.
        /// </param>
        protected void AddLuaReference(Lua lua)
        {
            lock (this.luaReferences)
            {
                this.luaReferences[lua.LuaState] = lua;
            }
        }

        /// <summary>
        /// Gets a Lua reference by a Lua state.
        /// </summary>
        /// <param name="luaState">
        /// A Lua state.
        /// </param>
        /// <returns>
        /// A Lua reference.
        /// </returns>
        protected Lua GetLuaReferenceByLuaState(IntPtr luaState)
        {
            lock (this.luaReferences)
            {
                return this.luaReferences[luaState];
            }
        }

        /// <summary>
        /// Handle a table index request (get a value).
        /// </summary>
        /// <param name="lua">
        /// A Lua reference.
        /// </param>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <param name="key">
        /// The index key.
        /// </param>
        protected abstract void IndexMetamethod(Lua lua, LuaTable table, LuaType key);

        /// <summary>
        /// Handle a table newindex request (set a value).
        /// </summary>
        /// <param name="lua">
        /// A Lua reference.
        /// </param>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <param name="key">
        /// The index key.
        /// </param>
        /// <param name="value">
        /// The new value.
        /// </param>
        protected abstract void NewIndexMetamethod(Lua lua, LuaTable table, LuaType key, LuaType value);

        private int IndexCFunctionHandler(IntPtr luaState)
        {
            var lua = this.GetLuaReferenceByLuaState(luaState);

            var oldStackCount = lua.Stack.Count;

            var inputList = lua.Stack.FetchCFunctionInputList();
            Debug.Assert(inputList.Count == 2, "asserting inputList.Count == 2");

            var table = (LuaTable)inputList[0];
            var key = inputList[1];

            this.IndexMetamethod(lua, table, key);

            Debug.Assert(lua.Stack.Count == oldStackCount + 1, "stack count");

            return 1;
        }

        private int NewIndexCFunctionHandler(IntPtr luaState)
        {
            var lua = this.GetLuaReferenceByLuaState(luaState);

            var inputList = lua.Stack.FetchCFunctionInputList();
            Debug.Assert(inputList.Count == 3, "asserting inputList.Count == 3");

            var table = (LuaTable)inputList[0];
            var key = inputList[1];
            var value = inputList[2];

            this.NewIndexMetamethod(lua, table, key, value);

            return 0;
        }
    }
}