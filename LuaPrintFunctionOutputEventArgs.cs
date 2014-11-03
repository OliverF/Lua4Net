namespace Lua4Net
{
    using System;

    public class LuaPrintFunctionOutputEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaPrintFunctionOutputEventArgs"/> class.
        /// </summary>
        /// <param name="text">
        /// The text to print.
        /// </param>
        public LuaPrintFunctionOutputEventArgs(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Gets the text to print.
        /// </summary>
        public string Text { get; private set; }
    }
}