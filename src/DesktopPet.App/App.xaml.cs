using DesktopPet.App.Animation;
using DesktopPet.App.SystemIntegration;
using System.Windows;
using System.Windows.Threading;

namespace DesktopPet.App;

public partial class App : System.Windows.Application
{
    private SingleInstanceGuard? _singleInstanceGuard;
    private IdleAnimationController? _idleAnimationController;
    private TrayService? _trayService;
    private PetWindow? _petWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceGuard = new SingleInstanceGuard(@"Local\DesktopPet.App");
        if (!_singleInstanceGuard.TryAcquire())
        {
            Shutdown(0);
            return;
        }

        try
        {
            _idleAnimationController = new IdleAnimationController();
            _petWindow = new PetWindow(
                _idleAnimationController,
                new DisplayCoordinateService());

            MainWindow = _petWindow;
            _trayService = new TrayService(
                showPet: () => Dispatcher.Invoke(ShowPet),
                hidePet: () => Dispatcher.Invoke(HidePet),
                exitApplication: () => Dispatcher.Invoke(Shutdown),
                isPetVisible: () => _petWindow?.IsVisible == true);

            _petWindow.Closed += (_, _) => Shutdown();
            _petWindow.Show();
            _idleAnimationController.Start();
            Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new Action(ReleaseStartupMemory));
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(
                $"桌宠启动失败：{exception.Message}",
                "DesktopPet",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ReleaseStartupMemory()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }

    private void ShowPet()
    {
        if (_petWindow is null || _idleAnimationController is null)
        {
            return;
        }

        _petWindow.Show();
        _petWindow.Topmost = true;
        _idleAnimationController.Start();
    }

    private void HidePet()
    {
        if (_petWindow is null || _idleAnimationController is null)
        {
            return;
        }

        _idleAnimationController.Pause();
        _petWindow.Hide();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        _idleAnimationController?.Dispose();
        _singleInstanceGuard?.Dispose();
        base.OnExit(e);
    }
}