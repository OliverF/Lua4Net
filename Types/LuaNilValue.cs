namespace Lua4Net.Types
{
    using System;

    public sealed class LuaNilValue : LuaValueType, IEquatable<LuaNilValue>
    {
        public bool Equals(LuaNilValue other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(LuaNilValue))
            {
                return false;
            }

            return this.Equals((LuaNilValue)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "nil";
        }
    }
}