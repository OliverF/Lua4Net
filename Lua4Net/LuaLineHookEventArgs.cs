namespace Lua4Net
{
    using System;

    public class LuaLineHookEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaLineHookEventArgs"/> class.
        /// </summary>
        /// <param name="debugContext">
        /// The debug context.
        /// </param>
        public LuaLineHookEventArgs(LuaDebugContext debugContext)
        {
            this.DebugContext = debugContext;
        }

        /// <summary>
        /// Gets the debug context.
        /// </summary>
        public LuaDebugContext DebugContext { get; private set; }
    }
}