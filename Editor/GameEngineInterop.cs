using System;
using System.Runtime.InteropServices;

namespace Editeur
{
    public class GameEngineInterop
    {
    #if WINDOWS
        private const string DLL_NAME = "pulsar-engine.dll";

        [DllImport(DLL_NAME, EntryPoint = "?destroyWindow@Engine@@SAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyWindow();

        [DllImport(DLL_NAME, EntryPoint = "?getWindowPtr@Engine@@SAPEAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetWindowPtr();

        [DllImport(DLL_NAME, EntryPoint = "?initWindow@Engine@@SAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitWindow(bool someParam);

        [DllImport(DLL_NAME, EntryPoint = "?render@Engine@@SAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Render();

        [DllImport(DLL_NAME, EntryPoint = "?togglePause@Engine@@SAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TogglePause();

        [DllImport(DLL_NAME, EntryPoint = "?toggleShow@Engine@@SAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ToggleShow();
        
        [DllImport(DLL_NAME, EntryPoint = "?close@Engine@@SAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseWindow();

    #elif LINUX
        private const string DLL_NAME = "libpulsar-engine.so";

        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine13destroyWindowEv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyWindow();

        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine12getWindowPtrEv", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetWindowPtr();

        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine10initWindowEb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitWindow(bool someParam);

        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine6renderEv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Render();

        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine11togglePauseEv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TogglePause();

        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine10toggleShowEv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ToggleShow();
        
        [DllImport(DLL_NAME, EntryPoint = "_ZN6Engine5closeEv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseWindow();
    #else
        private const string DLL_NAME = "libpulsar-engine.so";
    #endif
        
    }
}
