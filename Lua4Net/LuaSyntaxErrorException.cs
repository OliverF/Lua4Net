namespace Lua4Net
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class LuaSyntaxErrorException : LuaException
    {
        public LuaSyntaxErrorException()
        {
            this.Line = -1;
        }

        public LuaSyntaxErrorException(string message)
            : base(message)
        {
            this.Line = -1;
        }

        public LuaSyntaxErrorException(string message, Exception inner)
            : base(message, inner)
        {
            this.Line = -1;
        }

        public LuaSyntaxErrorException(string chunkName, int line, string message)
            : base(message)
        {
            this.ChunkName = chunkName;
            this.Line = line;
        }

        protected LuaSyntaxErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ChunkName = info.GetString("ChunkName");
            this.Line = info.GetInt32("Line");
        }

        public string ChunkName { get; private set; }
        public int Line { get; private set; }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ChunkName", this.ChunkName);
            info.AddValue("Line", this.Line);
        }
    }
}