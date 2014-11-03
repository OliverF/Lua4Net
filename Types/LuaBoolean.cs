namespace Lua4Net.Types
{
    using System;

    public sealed class LuaBoolean : LuaValueType, IEquatable<LuaBoolean>
    {
        public LuaBoolean(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; private set; }

        public bool Equals(LuaBoolean other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Value.Equals(this.Value);
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

            if (obj.GetType() != typeof(LuaBoolean))
            {
                return false;
            }

            return this.Equals((LuaBoolean)obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value ? "true" : "false";
        }
    }
}