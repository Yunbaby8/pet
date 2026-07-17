using System.IO;
using System.Windows;
using WpfImage = System.Windows.Controls.Image;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DesktopPet.App.Animation;

public sealed class IdleAnimationController : IDisposable
{
    private readonly Storyboard _storyboard = new();
    private WpfImage? _target;
    private bool _hasStarted;
    private bool _isPaused;

    public void Attach(WpfImage target)
    {
        ArgumentNullException.ThrowIfNull(target);

        string assetPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "DefaultPet",
            "pet.png");

        if (!File.Exists(assetPath))
        {
            throw new FileNotFoundException("找不到桌宠图片 Assets\\DefaultPet\\pet.png。", assetPath);
        }

        target.Source = LoadImage(assetPath);
        _target = target;
        BuildStoryboard(target);
    }

    public void Start()
    {
        if (_target is null)
        {
            throw new InvalidOperationException("待机动画尚未绑定桌宠图像。");
        }

        if (!_hasStarted)
        {
            _storyboard.Begin(_target, HandoffBehavior.SnapshotAndReplace, true);
            _hasStarted = true;
            _isPaused = false;
            return;
        }

        if (_isPaused)
        {
            _storyboard.Resume(_target);
            _isPaused = false;
        }
    }

    public void Pause()
    {
        if (_target is null || !_hasStarted || _isPaused)
        {
            return;
        }

        _storyboard.Pause(_target);
        _isPaused = true;
    }

    public void Dispose()
    {
        if (_target is not null && _hasStarted)
        {
            _storyboard.Remove(_target);
        }

        _target = null;
        _hasStarted = false;
        _isPaused = false;
    }

    private void BuildStoryboard(WpfImage target)
    {
        _storyboard.Children.Clear();
        var duration = new Duration(TimeSpan.FromSeconds(1.6));
        var easing = new SineEase { EasingMode = EasingMode.EaseInOut };

        AddAnimation(target, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)", 1.0, 1.02, duration, easing);
        AddAnimation(target, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)", 1.0, 1.02, duration, easing);
        AddAnimation(target, "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)", 0.0, -5.0, duration, easing);
    }

    private void AddAnimation(
        WpfImage target,
        string propertyPath,
        double from,
        double to,
        Duration duration,
        IEasingFunction easing)
    {
        var animation = new DoubleAnimation(from, to, duration)
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = easing
        };

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(propertyPath));
        _storyboard.Children.Add(animation);
    }

    private static BitmapImage LoadImage(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = 260;
        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }
}