namespace Lua4Net.Types
{
    using System.Diagnostics;

    public abstract class LuaTypeWithStackReference : LuaType
    {
        private readonly bool stackReferenceCheck;

        protected LuaTypeWithStackReference(Lua lua, int stackReferenceIndex, bool stackReferenceCheck)
        {
            Trace.Assert(lua != null);

            this.Lua = lua;
            this.StackReferenceIndex = stackReferenceIndex;
            this.stackReferenceCheck = stackReferenceCheck;
        }

        internal Lua Lua { get; private set; }

        internal int StackReferenceIndex { get; private set; }

        /// <summary>
        /// Checks lua reference and stack reference index (and throws exceptions).
        /// </summary>
        internal void CheckLuaReferenceAndStackIndex()
        {
            if (this.stackReferenceCheck)
            {
                if (!this.Lua.Stack.IsValidStackIndex(this.StackReferenceIndex))
                {
                    throw new LuaTypeMissingReferenceException("Stack reference index is not valid.");
                }

                if (this.Lua.Stack.GetEntryAtIndex(this.StackReferenceIndex).GetType() != this.GetType())
                {
                    throw new LuaTypeMissingReferenceException("Stack reference value is not a " + this.GetType().Name + ".");
                }
            }
        }
    }
}