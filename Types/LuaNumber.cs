namespace Lua4Net.Types
{
    using System;
    using System.Globalization;

    public sealed class LuaNumber : LuaValueType, IFormattable, IEquatable<LuaNumber>
    {
        public LuaNumber(double value)
        {
            this.Value = value;
        }

        public double Value { get; private set; }

        public bool Equals(LuaNumber other)
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

            if (obj.GetType() != typeof(LuaNumber))
            {
                return false;
            }

            return this.Equals((LuaNumber)obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return this.Value.ToString(format, formatProvider);
        }
    }
}