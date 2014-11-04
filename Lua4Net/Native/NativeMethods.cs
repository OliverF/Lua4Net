namespace Lua4Net.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <summary>
    /// Native lua type enum.
    /// </summary>
    internal enum NativeLuaTypes
    {
        /// <summary>
        /// Lua pseudo type LUA_TNONE.
        /// </summary>
        None = -1, 

        /// <summary>
        /// Lua type LUA_TNIL.
        /// </summary>
        Nil = 0, 

        /// <summary>
        /// Lua type LUA_TBOOLEAN.
        /// </summary>
        Boolean = 1, 

        /// <summary>
        /// Lua type LUA_TLIGHTUSERDATA.
        /// </summary>
        Lightuserdata = 2, 

        /// <summary>
        /// Lua type LUA_TNUMBER.
        /// </summary>
        Number = 3, 

        /// <summary>
        /// Lua type LUA_TSTRING.
        /// </summary>
        String = 4, 

        /// <summary>
        /// Lua type LUA_TTABLE.
        /// </summary>
        Table = 5, 

        /// <summary>
        /// Lua type LUA_TFUNCTION.
        /// </summary>
        Function = 6, 

        /// <summary>
        /// Lua type LUA_TUSERDATA.
        /// </summary>
        Userdata = 7, 

        /// <summary>
        /// Lua type LUA_TTHREAD.
        /// </summary>
        Thread = 8
    }

    internal enum NativeLuaHoookEventCodes
    {
        /// <summary>
        /// Lua event code LUA_HOOKCALL.
        /// </summary>
        LuaHookcall = 0, 

        /// <summary>
        /// Lua event code LUA_HOOKRET.
        /// </summary>
        LuaHookret = 1, 

        /// <summary>
        /// Lua event code LUA_HOOKLINE.
        /// </summary>
        LuaHookline = 2, 

        /// <summary>
        /// Lua event code LUA_HOOKCOUNT.
        /// </summary>
        LuaHookcount = 3, 

        /// <summary>
        /// Lua event code LUA_HOOKTAILRET.
        /// </summary>
        LuaHooktailret = 4
    }

    [Flags]
    internal enum NativeLuaHookEventMasks
    {
        /// <summary>
        /// Lua event mask LUA_MASKCALL.
        /// </summary>
        LuaMaskcall = 1 << NativeLuaHoookEventCodes.LuaHookcall, 

        /// <summary>
        /// Lua event mask LUA_MASKRET.
        /// </summary>
        LuaMaskret = 1 << NativeLuaHoookEventCodes.LuaHookret, 

        /// <summary>
        /// Lua event mask LUA_MASKLINE.
        /// </summary>
        LuaMaskline = 1 << NativeLuaHoookEventCodes.LuaHookline, 

        /// <summary>
        /// Lua event mask LUA_MASKCOUNT.
        /// </summary>
        LuaMaskcount = 1 << NativeLuaHoookEventCodes.LuaHookcount
    }

    internal static class NativeMethods
    {
        ////TODO: maybe we can replace get/set field by get/set table
        internal const int LuaErrrun = 2;

        internal const int LuaErrsyntax = 3;

        internal const int LuaErrmem = 4;

        internal const int LuaMultret = -1;

        internal const int LuaIdsize = 60;

        internal const int LuaRegistryindex = -10000;

        internal const int LuaEnvironindex = -10001;

        internal const int LuaGlobalsindex = -10002;

        /* LUA 5.2
        internal const int LuaRegistryindex = (-1000000 - 1000);
        internal const int LuaEnvironindex = LuaRegistryindex - 1;
        */
        private const string LuaDllName = "liblua51";

        private const CallingConvention DllCallingConv = CallingConvention.Cdecl;

        private const CharSet DllCharSet = CharSet.Ansi;

        [UnmanagedFunctionPointer(DllCallingConv)]
        internal delegate int LuaCFunctionDelegate(IntPtr luaState);

        [UnmanagedFunctionPointer(DllCallingConv)]
        internal delegate void LuaHookFunctionDelegate(IntPtr luaState, ref NativeLuaDebug luaDebug);

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal static string StringPointerToString(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal static string StringPointerToString(IntPtr ptr, int len)
        {
            return Marshal.PtrToStringAnsi(ptr, len);
        }

        #region lua interface

        /*[DllImport(LuaDllName, EntryPoint = "lua_pushlightuserdata", CallingConvention = DllCallingConv)]
        internal static extern void Lua_pushlightuserdata(IntPtr luaState, IntPtr p);

        [DllImport(LuaDllName, EntryPoint = "lua_newuserdata", CallingConvention = DllCallingConv)]
        internal static extern IntPtr Lua_newuserdata(IntPtr luaState, /* size_t * / IntPtr size);*/
        [DllImport(LuaDllName, EntryPoint = "lua_atpanic", CallingConvention = DllCallingConv)]
        internal static extern void Lua_atpanic(IntPtr luaState, LuaCFunctionDelegate panicf);

        [DllImport(LuaDllName, EntryPoint = "lua_call", CallingConvention = DllCallingConv)]
        internal static extern void Lua_call(IntPtr luaState, int nargs, int nresults);

        /* LUA 5.2
        internal static void Lua_call(IntPtr luaState, int nargs, int nresults)
        {
            lua_callk(luaState, nargs, nresults, 0, null);
        }

        [DllImport(LuaDllName, EntryPoint = "lua_callk", CallingConvention = DllCallingConv)]
        internal static extern void lua_callk(IntPtr luaState, int nargs, int nresults, int ctx, LuaCFunctionDelegate k);
        */
        [DllImport(LuaDllName, EntryPoint = "lua_close", CallingConvention = DllCallingConv)]
        internal static extern void Lua_close(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "lua_createtable", CallingConvention = DllCallingConv)]
        internal static extern void Lua_createtable(IntPtr luaState, int narr, int nrec);

        [DllImport(LuaDllName, EntryPoint = "lua_error", CallingConvention = DllCallingConv)]
        internal static extern int Lua_error(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "lua_getfield", CallingConvention = DllCallingConv, CharSet = DllCharSet, BestFitMapping = false)]
        internal static extern void Lua_getfield(IntPtr luaState, int index, string k);

        [DllImport(LuaDllName, EntryPoint = "lua_gettable", CallingConvention = DllCallingConv)]
        internal static extern void Lua_gettable(IntPtr luaState, int index);

        internal static void Lua_getglobal(IntPtr luaState, string name)
        {
            Lua_getfield(luaState, LuaGlobalsindex, name);
        }

        [DllImport(LuaDllName, EntryPoint = "lua_gettop", CallingConvention = DllCallingConv)]
        internal static extern int Lua_gettop(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "lua_next", CallingConvention = DllCallingConv)]
        internal static extern int Lua_next(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_objlen", CallingConvention = DllCallingConv)]
        /* return value: size_t */
        internal static extern IntPtr Lua_objlen(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_pcall", CallingConvention = DllCallingConv)]
        internal static extern int Lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

        /* LUA 5.2
        internal static int Lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc)
        {
            return Lua_pcallk(luaState, nargs, nresults, errfunc, 0, null);
        }

        [DllImport(LuaDllName, EntryPoint = "lua_pcallk", CallingConvention = DllCallingConv)]
        internal static extern int Lua_pcallk(IntPtr luaState, int nargs, int nresults, int errfunc, int ctx, LuaCFunctionDelegate k);
        */
        internal static void Lua_pop(IntPtr luaState, int n)
        {
            Lua_settop(luaState, (-n) - 1);
        }

        [DllImport(LuaDllName, EntryPoint = "lua_pushboolean", CallingConvention = DllCallingConv)]
        internal static extern void Lua_pushboolean(IntPtr luaState, [MarshalAs(UnmanagedType.Bool)] bool b);

        [DllImport(LuaDllName, EntryPoint = "lua_pushcclosure", CallingConvention = DllCallingConv)]
        internal static extern void Lua_pushcclosure(IntPtr luaState, LuaCFunctionDelegate f, int n);

        [DllImport(LuaDllName, EntryPoint = "lua_pushlstring", CallingConvention = DllCallingConv, CharSet = DllCharSet, BestFitMapping = false)]
        internal static extern void Lua_pushlstring(IntPtr luaState, string s, /* size_t */ IntPtr len);

        [DllImport(LuaDllName, EntryPoint = "lua_pushnil", CallingConvention = DllCallingConv)]
        internal static extern void Lua_pushnil(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "lua_pushnumber", CallingConvention = DllCallingConv)]
        internal static extern void Lua_pushnumber(IntPtr luaState, double n);

        [DllImport(LuaDllName, EntryPoint = "lua_pushvalue", CallingConvention = DllCallingConv)]
        internal static extern void Lua_pushvalue(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_settop", CallingConvention = DllCallingConv)]
        internal static extern void Lua_settop(IntPtr luaState, int newTop);

        [DllImport(LuaDllName, EntryPoint = "lua_setfield", CallingConvention = DllCallingConv, CharSet = DllCharSet, BestFitMapping = false)]
        internal static extern void Lua_setfield(IntPtr luaState, int index, string k);

        internal static void Lua_setglobal(IntPtr luaState, string name)
        {
            Lua_setfield(luaState, LuaGlobalsindex, name);
        }

        [DllImport(LuaDllName, EntryPoint = "lua_setmetatable", CallingConvention = DllCallingConv)]
        internal static extern void Lua_setmetatable(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_settable", CallingConvention = DllCallingConv)]
        internal static extern void Lua_settable(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_toboolean", CallingConvention = DllCallingConv)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Lua_toboolean(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_tolstring", CallingConvention = DllCallingConv)]
        internal static extern IntPtr Lua_tolstring(IntPtr luaState, int index, /* size_t */ out IntPtr len);

        [DllImport(LuaDllName, EntryPoint = "lua_tonumber", CallingConvention = DllCallingConv)]
        internal static extern double Lua_tonumber(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_topointer", CallingConvention = DllCallingConv)]
        internal static extern IntPtr Lua_topointer(IntPtr luaState, int index);

        [DllImport(LuaDllName, EntryPoint = "lua_type", CallingConvention = DllCallingConv)]
        internal static extern NativeLuaTypes Lua_type(IntPtr luaState, int index);

        #endregion

        #region lua auxiliary interface

        [DllImport(LuaDllName, EntryPoint = "luaL_loadbuffer", CallingConvention = DllCallingConv, CharSet = DllCharSet, BestFitMapping = false)]
        internal static extern int LuaL_loadbuffer(IntPtr luaState, string buff, /* size_t */ IntPtr sz, string name);

        [DllImport(LuaDllName, EntryPoint = "luaL_newstate", CallingConvention = DllCallingConv)]
        internal static extern IntPtr LuaL_newstate();

        #endregion

        #region lua debug interface

        [DllImport(LuaDllName, EntryPoint = "lua_getinfo", CallingConvention = DllCallingConv, CharSet = DllCharSet, BestFitMapping = false)]
        internal static extern int Lua_getinfo(IntPtr luaState, string what, ref NativeLuaDebug luaDebug);

        [DllImport(LuaDllName, EntryPoint = "lua_getlocal", CallingConvention = DllCallingConv)]
        internal static extern IntPtr Lua_getlocal(IntPtr luaState, ref NativeLuaDebug luaDebug, int n);

        [DllImport(LuaDllName, EntryPoint = "lua_sethook", CallingConvention = DllCallingConv)]
        internal static extern int Lua_sethook(IntPtr luaState, LuaHookFunctionDelegate f, NativeLuaHookEventMasks mask, int count);

        #endregion

        #region lua open library prodecdures

        [DllImport(LuaDllName, EntryPoint = "luaopen_base", CallingConvention = DllCallingConv)]
        internal static extern int Luaopen_base(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "luaopen_string", CallingConvention = DllCallingConv)]
        internal static extern int Luaopen_string(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "luaopen_table", CallingConvention = DllCallingConv)]
        internal static extern int Luaopen_table(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "luaopen_math", CallingConvention = DllCallingConv)]
        internal static extern int Luaopen_math(IntPtr luaState);

        [DllImport(LuaDllName, EntryPoint = "luaL_openlibs", CallingConvention = DllCallingConv)]
        internal static extern int luaL_openlibs(IntPtr luaState);

        #endregion
    }
}