using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua4Net.Types;

namespace Lua4Net
{
    public class LuaGlobalObjectValues
    {
        private LuaGlobalObjects globals;

        internal LuaGlobalObjectValues(LuaGlobalObjects globals)
            : base()
        {
            this.globals = globals;
        }

        public LuaValueObject this[string name]
        {
            get { return this.globals.GetGlobalValue(name); }
            set { this.globals.SetGlobalValue(name, value); }
        }
    }
}
