namespace Lua4Net.Types
{
    public class LuaFunction : LuaTypeWithStackReference
    {
        internal LuaFunction(Lua lua, int stackReferenceIndex)
            : base(lua, stackReferenceIndex, true)
        {
        }
    }
}