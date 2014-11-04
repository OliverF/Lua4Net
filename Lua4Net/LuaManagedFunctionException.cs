namespace Lua4Net
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class LuaManagedFunctionException : LuaException
    {
        public LuaManagedFunctionException()
        {
        }

        public LuaManagedFunctionException(string message)
            : base(message)
        {
        }

        public LuaManagedFunctionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected LuaManagedFunctionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}