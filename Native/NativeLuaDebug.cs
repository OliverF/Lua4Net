namespace Lua4Net.Native
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct NativeLuaDebug
    {
        internal readonly int EventCode;

        internal readonly IntPtr Name; // yes it's a string, but the pointer is not valid until lua_getinfo() will be called

        internal readonly IntPtr Namewhat; // - " -

        internal readonly IntPtr What; // - " -

        internal readonly IntPtr Source; // - " -

        internal readonly int Currentline;

        internal readonly int Nups;

        internal readonly int Linedefined;

        internal readonly int Lastlinedefined;

        // performace issue: string conversion
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.LuaIdsize)]
        internal readonly string Shortsrc;

        internal readonly int PrivateInt1;
    }
}