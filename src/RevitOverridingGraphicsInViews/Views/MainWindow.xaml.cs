using System.Media;
using System.Windows;
using System.Windows.Input;

namespace RevitOverridingGraphicsInViews.Views;
public partial class MainWindow {

    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitOverridingGraphicsInViews);
    public override string ProjectConfigName => nameof(MainWindow);


    private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

        if(e.ChangedButton == MouseButton.Left) {
            DragMove();
        }
    }

    private void WindowCloseCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {

        var player = new SoundPlayer {
            Stream = Properties.Resources.PaintSound
        };
        player.Play();

        Close();
    }


    private void ColorButton_Click(object sender, RoutedEventArgs e) {

        var player = new SoundPlayer {
            Stream = Properties.Resources.SelectColorSound
        };
        player.Play();
    }
}