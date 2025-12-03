using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPylonDocumentation.Views.Controls;

public partial class AppFooterControl : UserControl {
    public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(
        nameof(ErrorText), typeof(string), typeof(AppFooterControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty OkButtonTextProperty = DependencyProperty.Register(
        nameof(OkButtonText), typeof(string), typeof(AppFooterControl), new PropertyMetadata("Ok"));

    public static readonly DependencyProperty OkCommandProperty = DependencyProperty.Register(
        nameof(OkCommand), typeof(ICommand), typeof(AppFooterControl), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty ShowOkButtonProperty = DependencyProperty.Register(
        nameof(ShowOkButton), typeof(bool), typeof(AppFooterControl), new PropertyMetadata(true));

    public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register(
        nameof(CancelButtonText), typeof(string), typeof(AppFooterControl), new PropertyMetadata("Cancel"));


    public AppFooterControl() {
        InitializeComponent();
    }


    public string ErrorText {
        get => (string) GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public string OkButtonText {
        get => (string) GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    public ICommand OkCommand {
        get => (ICommand) GetValue(OkCommandProperty);
        set => SetValue(OkCommandProperty, value);
    }

    public bool ShowOkButton {
        get => (bool) GetValue(ShowOkButtonProperty);
        set => SetValue(ShowOkButtonProperty, value);
    }

    public string CancelButtonText {
        get => (string) GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = true;
        }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = false;
        }
    }
}
