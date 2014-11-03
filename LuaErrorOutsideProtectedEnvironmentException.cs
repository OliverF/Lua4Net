namespace Lua4Net
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class LuaErrorOutsideProtectedEnvironmentException : LuaException
    {
        public LuaErrorOutsideProtectedEnvironmentException()
        {
        }

        public LuaErrorOutsideProtectedEnvironmentException(string message)
            : base(message)
        {
        }

        public LuaErrorOutsideProtectedEnvironmentException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected LuaErrorOutsideProtectedEnvironmentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}