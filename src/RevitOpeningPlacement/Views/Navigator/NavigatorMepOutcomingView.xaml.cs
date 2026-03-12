using System.Windows;

using DevExpress.Xpf.Grid;

namespace RevitOpeningPlacement.Views.Navigator;
/// <summary>
/// Класс окна для отображения исходящих заданий на отверстия в файле инженерных систем
/// </summary>
public partial class NavigatorMepOutcomingView {
    /// <summary>
    /// Конструктор окна для отображения исходящих заданий на отверстия в файле инженерных систем
    /// </summary>
    public NavigatorMepOutcomingView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitOpeningPlacement);
    public override string ProjectConfigName => nameof(NavigatorMepOutcomingView);

    private void SimpleButton_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
