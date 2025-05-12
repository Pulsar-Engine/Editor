using Avalonia.Controls;
using Avalonia.Controls.Platform;
using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;

namespace Editeur
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            InitializeNativeControl();
        }

        private void InitializeNativeControl()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var handle = GetNativeHandle();
                if (handle != IntPtr.Zero)
                {
                    var method = typeof(NativeControlHost).GetMethod("SetNativeControlHostHandle",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method?.Invoke(NativeHost, new object[] { new PlatformHandle(handle, "HWND") });
                }
            }
        }

        private IntPtr GetNativeHandle()
        {
            return GameEngineInterop.GetWindowPtr();
        }
    }
}