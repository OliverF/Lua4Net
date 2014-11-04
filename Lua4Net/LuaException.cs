namespace Lua4Net
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class LuaException : Exception
    {
        public LuaException()
        {
        }

        public LuaException(string message)
            : base(message)
        {
        }

        public LuaException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected LuaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}