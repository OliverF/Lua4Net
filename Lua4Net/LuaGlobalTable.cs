namespace Lua4Net
{
    using Lua4Net.Native;
    using Lua4Net.Types;

    public class LuaGlobalTable : LuaTable
    {
        internal LuaGlobalTable(Lua lua)
            : base(lua, NativeMethods.LuaGlobalsindex, false)
        {
        }
    }
}