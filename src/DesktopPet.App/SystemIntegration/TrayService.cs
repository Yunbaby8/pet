using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace DesktopPet.App.SystemIntegration;

public sealed class TrayService : IDisposable
{
    private const int CallbackMessage = 0x8001;
    private const int WindowMessageLeftButtonDoubleClick = 0x0203;
    private const int WindowMessageRightButtonUp = 0x0205;
    private const int WindowMessageContextMenu = 0x007B;

    private const uint NotifyIconMessageAdd = 0x00000000;
    private const uint NotifyIconMessageDelete = 0x00000002;
    private const uint NotifyIconFlagMessage = 0x00000001;
    private const uint NotifyIconFlagIcon = 0x00000002;
    private const uint NotifyIconFlagTip = 0x00000004;
    private const int ApplicationIconId = 32512;

    private readonly Action _showPet;
    private readonly Action _hidePet;
    private readonly Action _exitApplication;
    private readonly Func<bool> _isPetVisible;
    private readonly HwndSource _messageWindow;
    private readonly ContextMenu _contextMenu;
    private readonly MenuItem _showItem;
    private readonly MenuItem _hideItem;
    private readonly uint _taskbarCreatedMessage;
    private NotifyIconData _notifyIconData;
    private bool _disposed;

    public TrayService(
        Action showPet,
        Action hidePet,
        Action exitApplication,
        Func<bool> isPetVisible)
    {
        _showPet = showPet;
        _hidePet = hidePet;
        _exitApplication = exitApplication;
        _isPetVisible = isPetVisible;

        var parameters = new HwndSourceParameters("DesktopPet.TrayMessageWindow")
        {
            Width = 0,
            Height = 0,
            WindowStyle = unchecked((int)0x80000000),
            ExtendedWindowStyle = 0x00000080
        };

        _messageWindow = new HwndSource(parameters);
        _messageWindow.AddHook(WindowProcedure);

        _showItem = new MenuItem { Header = "显示桌宠" };
        _hideItem = new MenuItem { Header = "隐藏桌宠" };
        var exitItem = new MenuItem { Header = "退出程序" };

        _showItem.Click += (_, _) => _showPet();
        _hideItem.Click += (_, _) => _hidePet();
        exitItem.Click += (_, _) => _exitApplication();

        _contextMenu = new ContextMenu
        {
            Placement = PlacementMode.MousePoint,
            StaysOpen = false
        };
        _contextMenu.Items.Add(_showItem);
        _contextMenu.Items.Add(_hideItem);
        _contextMenu.Items.Add(new Separator());
        _contextMenu.Items.Add(exitItem);
        WarmUpContextMenu();

        _notifyIconData = CreateNotifyIconData(_messageWindow.Handle);
        _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
        AddNotifyIcon();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _contextMenu.IsOpen = false;
        ShellNotifyIcon(NotifyIconMessageDelete, ref _notifyIconData);
        _messageWindow.RemoveHook(WindowProcedure);
        _messageWindow.Dispose();
        _disposed = true;
    }

    private nint WindowProcedure(
        nint windowHandle,
        int message,
        nint wordParameter,
        nint longParameter,
        ref bool handled)
    {
        if ((uint)message == _taskbarCreatedMessage)
        {
            AddNotifyIcon();
            handled = true;
            return nint.Zero;
        }

        if (message != CallbackMessage)
        {
            return nint.Zero;
        }

        int eventCode = unchecked((int)longParameter.ToInt64()) & 0xFFFF;
        switch (eventCode)
        {
            case WindowMessageLeftButtonDoubleClick:
                _showPet();
                handled = true;
                break;
            case WindowMessageRightButtonUp:
            case WindowMessageContextMenu:
                ShowContextMenu();
                handled = true;
                break;
        }

        return nint.Zero;
    }

    private void WarmUpContextMenu()
    {
        _contextMenu.Opacity = 0;
        _contextMenu.Placement = PlacementMode.AbsolutePoint;
        _contextMenu.HorizontalOffset = -32000;
        _contextMenu.VerticalOffset = -32000;
        _contextMenu.IsOpen = true;
        _contextMenu.UpdateLayout();
        _contextMenu.IsOpen = false;
        _contextMenu.Opacity = 1;
        _contextMenu.HorizontalOffset = 0;
        _contextMenu.VerticalOffset = 0;
        _contextMenu.Placement = PlacementMode.MousePoint;
    }
    private void ShowContextMenu()
    {
        bool isVisible = _isPetVisible();
        _showItem.IsEnabled = !isVisible;
        _hideItem.IsEnabled = isVisible;
        _contextMenu.IsOpen = true;
    }

    private void AddNotifyIcon()
    {
        if (!ShellNotifyIcon(NotifyIconMessageAdd, ref _notifyIconData))
        {
            throw new InvalidOperationException("无法创建系统托盘图标。");
        }
    }

    private static NotifyIconData CreateNotifyIconData(nint windowHandle)
    {
        return new NotifyIconData
        {
            Size = Marshal.SizeOf<NotifyIconData>(),
            WindowHandle = windowHandle,
            Id = 1,
            Flags = NotifyIconFlagMessage | NotifyIconFlagIcon | NotifyIconFlagTip,
            CallbackMessage = CallbackMessage,
            IconHandle = LoadIcon(nint.Zero, new nint(ApplicationIconId)),
            ToolTip = "DesktopPet v0.0.1",
            Info = string.Empty,
            InfoTitle = string.Empty
        };
    }

    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShellNotifyIcon(uint message, ref NotifyIconData data);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint LoadIcon(nint instance, nint iconName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string message);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public int Size;
        public nint WindowHandle;
        public uint Id;
        public uint Flags;
        public uint CallbackMessage;
        public nint IconHandle;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string ToolTip;

        public uint State;
        public uint StateMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Info;

        public uint TimeoutOrVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string InfoTitle;

        public uint InfoFlags;
        public Guid GuidItem;
        public nint BalloonIcon;
    }
}