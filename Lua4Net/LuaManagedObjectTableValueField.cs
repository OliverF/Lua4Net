namespace Lua4Net
{
    using System;

    using Lua4Net.Types;

    public class LuaManagedObjectTableValueField : LuaManagedObjectTableField, IEquatable<LuaManagedObjectTableValueField>, IFormattable
    {
        public LuaManagedObjectTableValueField(LuaValueType value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.Value = value;
        }

        public LuaValueType Value { get; private set; }

        #region Equality

        public bool Equals(LuaManagedObjectTableValueField other)
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

            if (obj.GetType() != typeof(LuaManagedObjectTableValueField))
            {
                return false;
            }

            return this.Equals((LuaManagedObjectTableValueField)obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        #endregion

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Convert.ToString(this.Value, formatProvider);
        }
    }
}