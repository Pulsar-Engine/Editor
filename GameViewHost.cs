using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;

namespace Editeur;

public class GameViewHost : NativeControlHost
{
    private IntPtr _nativeHandle;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _nativeHandle = GameEngineInterop.GetWindowPtr();
            return new PlatformHandle(_nativeHandle, "HWND");
        }

        throw new PlatformNotSupportedException();
    }
}
