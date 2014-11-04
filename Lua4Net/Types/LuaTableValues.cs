namespace Lua4Net.Types
{
    public class LuaTableValues
    {
        private readonly LuaTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaTableValues"/> class.
        /// </summary>
        /// <param name="table">
        /// The table.
        /// </param>
        internal LuaTableValues(LuaTable table)
        {
            this.table = table;
        }

        /// <summary>
        /// Gets or sets a table field as value type.
        /// </summary>
        /// <param name="path">
        /// A <see cref="LuaTablePath.TablePathSeparator"/> delimited path of value types (indices)
        ///   pointing to a table field.
        /// </param>
        /// <returns>
        /// A value type.
        /// </returns>
        public LuaValueType this[string path]
        {
            get
            {
                return this.table.GetFieldValue(LuaTablePath.StringToPath(path));
            }

            set
            {
                this.table.SetFieldValue(LuaTablePath.StringToPath(path), value);
            }
        }
    }
}