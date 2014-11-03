namespace Lua4Net
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class LuaRuntimeErrorException : LuaException
    {
        public LuaRuntimeErrorException()
        {
            this.Line = -1;
        }

        public LuaRuntimeErrorException(string message)
            : base(message)
        {
            this.Line = -1;
        }

        public LuaRuntimeErrorException(string message, Exception inner)
            : base(message, inner)
        {
            this.Line = -1;
        }

        public LuaRuntimeErrorException(int line, string message)
            : base(message)
        {
            this.Line = line;
        }

        protected LuaRuntimeErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Line = info.GetInt32("Line");
        }

        public int Line { get; private set; }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Line", this.Line);
        }
    }
}