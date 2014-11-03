namespace Lua4Net
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class LuaTypeMissingReferenceException : LuaException
    {
        public LuaTypeMissingReferenceException()
        {
        }

        public LuaTypeMissingReferenceException(string message)
            : base(message)
        {
        }

        public LuaTypeMissingReferenceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected LuaTypeMissingReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}