namespace Lua4Net
{
    /// <summary>
    /// Represents the handler (callback) of a <see cref="LuaManagedFunction"/>.
    /// </summary>
    /// <param name="args">
    /// The handler arguments.
    /// </param>
    public delegate void LuaManagedFunctionHandler(LuaManagedFunctionArgs args);
}