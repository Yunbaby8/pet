using DesktopPet.App.Animation;
using DesktopPet.App.SystemIntegration;
using System.Windows;
using System.Windows.Input;

namespace DesktopPet.App;

public partial class PetWindow : Window
{
    private readonly DisplayCoordinateService _displayCoordinateService;

    public PetWindow(
        IdleAnimationController idleAnimationController,
        DisplayCoordinateService displayCoordinateService)
    {
        InitializeComponent();
        _displayCoordinateService = displayCoordinateService;
        idleAnimationController.Attach(PetImage);

        SourceInitialized += OnSourceInitialized;
        DpiChanged += (_, _) => _displayCoordinateService.GetCurrentDisplay(this);
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _displayCoordinateService.PlaceAtPrimaryWorkAreaBottomRight(this, 24);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // DragMove can end between mouse messages; the window remains usable.
        }
    }
}