namespace Lua4Net.Types
{
    using System;

    public sealed class LuaString : LuaValueType, IEquatable<LuaString>
    {
        public LuaString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.Value = value;
        }

        public string Value { get; private set; }

        public bool Equals(LuaString other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return object.Equals(other.Value, this.Value);
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

            if (obj.GetType() != typeof(LuaString))
            {
                return false;
            }

            return this.Equals((LuaString)obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}