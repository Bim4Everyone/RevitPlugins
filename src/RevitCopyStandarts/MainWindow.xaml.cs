namespace RevitCopyStandarts;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitCopyStandarts);
    public override string ProjectConfigName => nameof(MainWindow);
}
