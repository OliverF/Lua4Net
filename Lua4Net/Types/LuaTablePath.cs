namespace Lua4Net.Types
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Static table path methods.
    /// </summary>
    public static class LuaTablePath
    {
        /// <summary>
        /// Separator character used in <see cref="LuaTablePath.StringToPath"/> to a table path.
        /// </summary>
        public const char TablePathSeparator = '.';

        /// <summary>
        /// Gets an empty table path.
        /// </summary>
        public static IEnumerable<LuaValueType> Empty
        {
            get
            {
                return new LuaValueType[0];
            }
        }

        /// <summary>
        /// Converts a period delimited table path string to a table path enumeration.
        /// </summary>
        /// <param name="path">
        /// A period delimited table path string.
        /// </param>
        /// <returns>
        /// A table path enumeration.
        /// </returns>
        public static IEnumerable<LuaValueType> StringToPath(string path)
        {
            var pathParts = path.Split(TablePathSeparator);

            var tablePath = pathParts.Select(p => (LuaValueType)new LuaString(p));

            return tablePath;
        }
    }
}