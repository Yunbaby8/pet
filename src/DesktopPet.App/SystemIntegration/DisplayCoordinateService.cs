using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopPet.App.SystemIntegration;

public sealed class DisplayCoordinateService
{
    private const uint MonitorDefaultToNearest = 2;

    public DisplayInfo GetCurrentDisplay(Window window)
    {
        nint handle = new WindowInteropHelper(window).Handle;
        nint monitor = MonitorFromWindow(handle, MonitorDefaultToNearest);
        var monitorInfo = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };

        if (monitor == nint.Zero || !GetMonitorInfo(monitor, ref monitorInfo))
        {
            throw new InvalidOperationException("无法获取当前显示器信息。");
        }

        uint dpi = handle == nint.Zero ? 96u : GetDpiForWindow(handle);
        if (dpi == 0)
        {
            dpi = 96;
        }

        return new DisplayInfo(
            monitorInfo.Monitor.Left,
            monitorInfo.Monitor.Top,
            monitorInfo.Monitor.Right,
            monitorInfo.Monitor.Bottom,
            monitorInfo.WorkArea.Left,
            monitorInfo.WorkArea.Top,
            monitorInfo.WorkArea.Right,
            monitorInfo.WorkArea.Bottom,
            dpi / 96d);
    }

    public void PlaceAtPrimaryWorkAreaBottomRight(Window window, double marginDip)
    {
        DisplayInfo display = GetCurrentDisplay(window);
        double scale = display.DpiScale;

        window.Left = display.WorkAreaRight / scale - window.Width - marginDip;
        window.Top = display.WorkAreaBottom / scale - window.Height - marginDip;
    }

    public sealed record DisplayInfo(
        int MonitorLeft,
        int MonitorTop,
        int MonitorRight,
        int MonitorBottom,
        int WorkAreaLeft,
        int WorkAreaTop,
        int WorkAreaRight,
        int WorkAreaBottom,
        double DpiScale);

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo monitorInfo);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint windowHandle);

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}