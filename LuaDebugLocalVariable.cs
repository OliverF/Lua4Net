namespace Lua4Net
{
    using Lua4Net.Types;

    public class LuaDebugLocalVariable
    {
        public LuaDebugLocalVariable(int index, string name, LuaType content)
        {
            this.Index = index;
            this.Name = name;
            this.Content = content;
        }

        public int Index { get; private set; }

        public string Name { get; private set; }

        public LuaType Content { get; private set; }
    }
}