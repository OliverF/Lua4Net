namespace Lua4Net.Types
{
    /// <summary>
    /// Specfifies the behavior of <see cref="LuaTable.GetLength(Lua4Net.Types.LuaTableGetLengthMethod)"/> and <see cref="LuaTable.GetLength(System.Collections.Generic.IEnumerable{Lua4Net.Types.LuaValueType},Lua4Net.Types.LuaTableGetLengthMethod)"/>.
    /// </summary>
    public enum LuaTableGetLengthMethod
    {
        /// <summary>
        /// Behave like the Lua <c>#</c>-operator.
        /// </summary>
        LuaLength, 

        /// <summary>
        /// Use the Lua <c>next</c>-function to determine the length.
        /// </summary>
        LuaNextCount
    }
}