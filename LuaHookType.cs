namespace Lua4Net
{
    /// <summary>
    /// Lua hook type.
    /// </summary>
    public enum LuaHookType
    {
        /// <summary>
        /// Hook won't be called.
        /// </summary>
        None, 

        /// <summary>
        /// Hook is called every line.
        /// </summary>
        LineHook
    }
}