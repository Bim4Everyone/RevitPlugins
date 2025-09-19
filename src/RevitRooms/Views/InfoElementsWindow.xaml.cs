namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for InfoElementsWindow.xaml
/// </summary>
public partial class InfoElementsWindow {
    public InfoElementsWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRooms);
    public override string ProjectConfigName => nameof(InfoElementsWindow);
}

internal enum TypeInfo {
    None,
    Info,
    Error,
    Warning,
}
