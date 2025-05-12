using System;
using System.Runtime.InteropServices;

namespace Editeur;

public class GameEngineInterop
{
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
}