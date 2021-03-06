﻿namespace Lua4Net
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

        public LuaRuntimeErrorException(string chunkName, int line, string message, string stackTrace)
            : base(message)
        {
            this.ChunkName = chunkName;
            this.Line = line;
            this.LuaStackTrace = stackTrace;
        }

        protected LuaRuntimeErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ChunkName = info.GetString("ChunkName");
            this.Line = info.GetInt32("Line");
            this.LuaStackTrace = info.GetString("LuaStackTrace");
        }

        public string ChunkName { get; private set; }
        public int Line { get; private set; }
        public string LuaStackTrace { get; private set; }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ChunkName", this.ChunkName);
            info.AddValue("Line", this.Line);
            info.AddValue("LuaStackTrace", this.LuaStackTrace);
        }
    }
}