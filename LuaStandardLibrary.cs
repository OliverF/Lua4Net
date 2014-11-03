namespace Lua4Net
{
    /// <summary>
    /// Lua standard library enumeration.
    /// </summary>
    public enum LuaStandardLibrary
    {
        /// <summary>
        /// Base library (get/setmetatable, load, error, ...).
        /// </summary>
        Base, 

        /// <summary>
        /// String library (string.byte, string.char, string.find, ...).
        /// </summary>
        String, 

        /// <summary>
        /// Table library (table.concat, ...).
        /// </summary>
        Table, 

        /// <summary>
        /// Math library (math.abs, math.exp, ...).
        /// </summary>
        Math, 
    }
}