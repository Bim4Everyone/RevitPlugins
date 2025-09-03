namespace RevitRoomExtrusion.Views;

public partial class ErrorWindow {

    public ErrorWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRoomExtrusion);
    public override string ProjectConfigName => nameof(ErrorWindow);
}
