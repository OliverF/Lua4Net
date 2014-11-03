namespace Lua4Net
{
    using System.Collections.Generic;

    using Lua4Net.Types;

    /// <summary>
    /// Represents the arguments of a <see cref="LuaManagedFunctionHandler"/>.
    /// </summary>
    public class LuaManagedFunctionArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaManagedFunctionArgs"/> class.
        /// </summary>
        /// <param name="lua">
        /// The Lua reference the function is called from.
        /// </param>
        /// <param name="input">
        /// The input arguments.
        /// </param>
        /// <param name="output">
        /// The output result.
        /// </param>
        internal LuaManagedFunctionArgs(Lua lua, IList<LuaType> input, IList<LuaValueType> output)
        {
            this.Lua = lua;
            this.Input = input;
            this.Output = output;
        }

        /// <summary>
        /// Gets the Lua reference the function is called from.
        /// </summary>
        /// <value>
        /// The Lua reference the function is called from.
        /// </value>
        public Lua Lua { get; private set; }

        /// <summary>
        /// Gets the input arguments.
        /// </summary>
        /// <value>
        /// The input arguments.
        /// </value>
        public IList<LuaType> Input { get; private set; }

        /// <summary>
        /// Gets the output result.
        /// </summary>
        /// <value>
        /// The output result.
        /// </value>
        public IList<LuaValueType> Output { get; private set; }
    }
}