using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PopupModalDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) CloseAll(); };
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    private void MinBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void OpenLeftPanel_Click(object sender, RoutedEventArgs e)
    {
        leftPanel.Visibility = Visibility.Visible;
        AnimateTranslate(leftPanelTranslate, -280, 0);
    }

    private void CloseLeftPanel_Click(object sender, RoutedEventArgs e)
        => AnimateTranslate(leftPanelTranslate, 0, -280, () => leftPanel.Visibility = Visibility.Collapsed);

    private void OpenRightPanel_Click(object sender, RoutedEventArgs e)
    {
        rightPanel.Visibility = Visibility.Visible;
        AnimateTranslate(rightPanelTranslate, 300, 0);
    }

    private void CloseRightPanel_Click(object sender, RoutedEventArgs e)
        => AnimateTranslate(rightPanelTranslate, 0, 300, () => rightPanel.Visibility = Visibility.Collapsed);

    private void ShowConfirm_Click(object sender, RoutedEventArgs e) => ShowModal(confirmModal);
    private void ShowInfo_Click(object sender, RoutedEventArgs e) => ShowModal(infoModal);
    private void ShowInput_Click(object sender, RoutedEventArgs e) => ShowModal(inputModal);

    private void ConfirmCancel_Click(object sender, RoutedEventArgs e) => CloseModal(confirmModal);
    private void ConfirmOk_Click(object sender, RoutedEventArgs e) => CloseModal(confirmModal);
    private void InfoClose_Click(object sender, RoutedEventArgs e) => CloseModal(infoModal);
    private void InputCancel_Click(object sender, RoutedEventArgs e) => CloseModal(inputModal);
    private void InputOk_Click(object sender, RoutedEventArgs e) => CloseModal(inputModal);

    private void Overlay_MouseDown(object sender, MouseButtonEventArgs e) => CloseAll();

    private void ShowModal(UIElement modal)
    {
        overlay.Visibility = Visibility.Visible;
        modal.Visibility = Visibility.Visible;
    }

    private void CloseModal(UIElement modal)
    {
        modal.Visibility = Visibility.Collapsed;
        overlay.Visibility = Visibility.Collapsed;
    }

    private void CloseAll()
    {
        foreach (var modal in new UIElement[] { confirmModal, infoModal, inputModal })
            modal.Visibility = Visibility.Collapsed;
        overlay.Visibility = Visibility.Collapsed;
        AnimateTranslate(leftPanelTranslate, 0, -280, () => leftPanel.Visibility = Visibility.Collapsed);
        AnimateTranslate(rightPanelTranslate, 0, 300, () => rightPanel.Visibility = Visibility.Collapsed);
    }

    private void ShowSuccessToast_Click(object sender, RoutedEventArgs e)
    {
        toastIcon.Text = "✅";
        toastTitle.Text = "Mission Accomplished";
        toastTitle.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 148));
        toastBody.Text = "Target neutralized. Shields holding at 94%.";
        toastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 148));
        ShowToast();
    }

    private void ShowErrorToast_Click(object sender, RoutedEventArgs e)
    {
        toastIcon.Text = "❌";
        toastTitle.Text = "Hull Breach Detected";
        toastTitle.Foreground = new SolidColorBrush(Color.FromRgb(255, 45, 85));
        toastBody.Text = "Deck 7 Section 4. Repair crews dispatched.";
        toastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 45, 85));
        ShowToast();
    }

    private async void ShowToast()
    {
        toastBorder.Visibility = Visibility.Visible;
        toastBorder.Opacity = 1;
        await System.Threading.Tasks.Task.Delay(3000);
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
        fade.Completed += (_, _) => toastBorder.Visibility = Visibility.Collapsed;
        toastBorder.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    private static void AnimateTranslate(TranslateTransform t, double from, double to, Action? onComplete = null)
    {
        var anim = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(250)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        if (onComplete != null) anim.Completed += (_, _) => onComplete();
        t.BeginAnimation(TranslateTransform.XProperty, anim);
    }
}
