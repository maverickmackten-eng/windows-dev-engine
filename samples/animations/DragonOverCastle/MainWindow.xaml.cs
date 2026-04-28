using System.Windows;
using System.Windows.Input;

namespace DragonOverCastle;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
        if (e.Key == Key.Space) TriggerFireBreath();
        if (e.Key == Key.R) RestartAnimation();
    }

    private void TriggerFireBreath()
    {
        // Immediate fire breath on demand — opacity spike
        var anim = new System.Windows.Media.Animation.DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new System.Windows.Media.Animation.EasingDoubleKeyFrame(
            1, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.05))));
        anim.KeyFrames.Add(new System.Windows.Media.Animation.EasingDoubleKeyFrame(
            0.7, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.4))));
        anim.KeyFrames.Add(new System.Windows.Media.Animation.EasingDoubleKeyFrame(
            0, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8))));
        fireBreath.BeginAnimation(OpacityProperty, anim);
    }

    private void RestartAnimation()
    {
        mainStoryboard.Stop();
        mainStoryboard.Begin();
    }
}
