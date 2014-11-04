namespace Lua4Net.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Lua4Net.Native;

    public class LuaTable : LuaTypeWithStackReference
    {
        internal LuaTable(Lua lua, int stackReferenceIndex, bool stackReferenceCheck)
            : base(lua, stackReferenceIndex, stackReferenceCheck)
        {
            this.Values = new LuaTableValues(this);
        }

        /// <summary>
        /// Gets the table field access helper.
        /// </summary>
        public LuaTableValues Values { get; private set; }

        /// <summary>
        /// Get a table field.
        /// </summary>
        /// <param name="path">
        /// A path of value types (indices) pointing to a table field.
        /// </param>
        /// <returns>
        /// The field.
        /// </returns>
        public LuaType GetField(IEnumerable<LuaValueType> path)
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                int pathCount;
                this.PathToStack(path, true, true, out pathCount);

                LuaType result;

                try
                {
                    result = this.Lua.Stack.Top;
                }
                finally
                {
                    this.Lua.Stack.PopAndForgetEntries(pathCount);
                }

                return result;
            }
        }

        /// <summary>
        /// Get a table field and check if it is a value type (and cast it).
        /// </summary>
        /// <param name="path">
        /// A path of value types (indices) pointing to a table field.
        /// </param>
        /// <returns>
        /// The field as value type.
        /// </returns>
        public LuaValueType GetFieldValue(IEnumerable<LuaValueType> path)
        {
            var fieldAsValueType = this.GetField(path) as LuaValueType;

            if (fieldAsValueType == null)
            {
                throw new LuaException("Field at the given path is not a value type.");
            }

            return fieldAsValueType;
        }

        /// <summary>
        /// Set a lua value to a table field.
        /// </summary>
        /// <param name="path">
        /// A path of value types (indices) pointing to a table field.
        /// </param>
        /// <param name="value">
        /// The new field value.
        /// </param>
        public void SetFieldValue(IEnumerable<LuaValueType> path, LuaValueType value)
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                if (value == null)
                {
                    value = new LuaNilValue();
                }

                int pathCount;
                var lastPart = this.PathToStack(path, false, true, out pathCount);

                try
                {
                    int tableIndex;

                    if (pathCount == 1)
                    {
                        tableIndex = this.StackReferenceIndex;
                    }
                    else
                    {
                        tableIndex = this.Lua.Stack.Count;
                    }

                    if (this.Lua.Stack.IsNilValueAtIndex(tableIndex))
                    {
                        throw new LuaException("Cannot set a value to a nil-table.");
                    }

                    this.Lua.Stack.PushValue(lastPart);
                    this.Lua.Stack.PushValue(value);
                    NativeMethods.Lua_settable(this.Lua.LuaState, tableIndex);
                }
                finally
                {
                    this.Lua.Stack.PopAndForgetEntries(pathCount - 1);
                }
            }
        }

        /// <summary>
        /// Get the length of the table.
        /// </summary>
        /// <param name="method">
        /// The method how to get the length.
        /// </param>
        /// <returns>
        /// The table length.
        /// </returns>
        public int GetLength(LuaTableGetLengthMethod method)
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                int length;

                if (method == LuaTableGetLengthMethod.LuaLength)
                {
                    length = NativeMethods.Lua_objlen(this.Lua.LuaState, this.StackReferenceIndex).ToInt32();
                }
                else
                {
                    length = 0;

                    this.Lua.Stack.PushValue(new LuaNilValue());

                    while (NativeMethods.Lua_next(this.Lua.LuaState, this.StackReferenceIndex) != 0)
                    {
                        this.Lua.Stack.PopAndForgetEntries(1);

                        length++;
                    }
                }

                return length;
            }
        }

        /// <summary>
        /// Get the length of the table.
        /// </summary>
        /// <param name="path">
        /// A path of value types (indices) pointing to a sub table.
        /// </param>
        /// <param name="method">
        /// The method how to get the length.
        /// </param>
        /// <returns>
        /// The table length.
        /// </returns>
        public int GetLength(IEnumerable<LuaValueType> path, LuaTableGetLengthMethod method)
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                int pathCount;
                this.PathToStack(path, true, false, out pathCount);

                int length;

                if (pathCount == 0)
                {
                    length = this.GetLength(method);
                }
                else
                {
                    try
                    {
                        LuaTable topAsTable = this.GetStackTopAsTableAfterPathToStack();

                        length = topAsTable.GetLength(method);
                    }
                    finally
                    {
                        this.Lua.Stack.PopAndForgetEntries(pathCount);
                    }
                }

                return length;
            }
        }

        /// <summary>
        /// Create a new sub table.
        /// </summary>
        /// <param name="path">
        /// A path of value types (indices) pointing to a new (non existent) sub table.
        /// </param>
        public void CreateSubTable(IEnumerable<LuaValueType> path)
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                int pathCount;
                var lastPart = this.PathToStack(path, false, true, out pathCount);

                try
                {
                    int tableIndex;

                    if (pathCount == 1)
                    {
                        tableIndex = this.StackReferenceIndex;
                    }
                    else
                    {
                        tableIndex = this.Lua.Stack.Count;
                    }

                    if (this.Lua.Stack.IsNilValueAtIndex(tableIndex))
                    {
                        throw new LuaException("Cannot create a sub table to a nil value.");
                    }

                    this.Lua.Stack.PushValue(lastPart);
                    NativeMethods.Lua_createtable(this.Lua.LuaState, 0, 0);
                    NativeMethods.Lua_settable(this.Lua.LuaState, tableIndex);
                }
                finally
                {
                    this.Lua.Stack.PopAndForgetEntries(pathCount - 1);
                }
            }
        }

        /// <summary>
        /// Fetches all table fields and returns them in a dictionary (just value types as keys allowed).
        /// </summary>
        /// <returns>
        /// The table fields dictionary.
        /// </returns>
        public IDictionary<LuaValueType, LuaType> FetchTableFields()
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                this.Lua.Stack.PushValue(new LuaNilValue());

                var dictionary = new Dictionary<LuaValueType, LuaType>();

                while (NativeMethods.Lua_next(this.Lua.LuaState, this.StackReferenceIndex) != 0)
                {
                    // on stack is now key and value
                    var value = this.Lua.Stack.Pop();

                    // key stays on the stack
                    var key = this.Lua.Stack.Top;

                    var keyAsValueType = key as LuaValueType;

                    if (keyAsValueType == null)
                    {
                        // remove key from stack before throwing exception
                        this.Lua.Stack.PopAndForgetEntries(1);

                        throw new LuaException("Table contains an object type key.");
                    }

                    dictionary[keyAsValueType] = value;
                }

                return dictionary;
            }
        }

        /// <summary>
        /// Fetches all table fields from a sub table and returns them in a dictionary (just value types as keys allowed).
        /// </summary>
        /// <param name="path">
        /// A path of value types (indices) pointing to a sub table.
        /// </param>
        /// <returns>
        /// The table fields dictionary.
        /// </returns>
        public IDictionary<LuaValueType, LuaType> FetchTableFields(IEnumerable<LuaValueType> path)
        {
            this.CheckLuaReferenceAndStackIndex();

            using (this.Lua.Stack.CreateCountChecker(0))
            {
                int pathCount;
                this.PathToStack(path, true, false, out pathCount);

                IDictionary<LuaValueType, LuaType> dictionary;

                if (pathCount == 0)
                {
                    dictionary = this.FetchTableFields();
                }
                else
                {
                    try
                    {
                        LuaTable topAsTable = this.GetStackTopAsTableAfterPathToStack();

                        dictionary = topAsTable.FetchTableFields();
                    }
                    finally
                    {
                        this.Lua.Stack.PopAndForgetEntries(pathCount);
                    }
                }

                return dictionary;
            }
        }

        private LuaValueType PathToStack(IEnumerable<LuaValueType> path, bool lastPartToo, bool throwExceptionIfEmpty, out int pathCount)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            pathCount = path.Count();

            if (throwExceptionIfEmpty && pathCount <= 0)
            {
                throw new ArgumentException("Path is empty", "path");
            }

            var currStackIndex = this.StackReferenceIndex;

            ////TODO: call lua checkstack

            for (int i = 0; i < (lastPartToo ? pathCount : pathCount - 1); i++)
            {
                // check for nil value
                if (this.Lua.Stack.IsNilValueAtIndex(currStackIndex))
                {
                    this.Lua.Stack.PopAndForgetEntries(i);
                    throw new LuaException("Cannot get a field of a nil value.");
                }

                this.Lua.Stack.PushValue(path.ElementAt(i));

                NativeMethods.Lua_gettable(this.Lua.LuaState, currStackIndex);

                // now this field is on the stack
                currStackIndex = this.Lua.Stack.Count;
            }

            return path.LastOrDefault();
        }

        private LuaTable GetStackTopAsTableAfterPathToStack()
        {
            var topAsTable = this.Lua.Stack.Top as LuaTable;

            if (topAsTable == null)
            {
                throw new LuaException("Field at the given path is not a table.");
            }

            return topAsTable;
        }
    }
}