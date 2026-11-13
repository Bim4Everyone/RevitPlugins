using System.Windows;

using DevExpress.Xpf.Grid;

namespace RevitOpeningPlacement.Views.Navigator;

/// <summary>
/// Класс окна для отображения входящих заданий на отверстия в файле архитектора или конструктора
/// </summary>
public partial class NavigatorMepToArIncomingView {
    /// <summary>
    /// Конструктор окна для отображения входящих заданий на отверстия в файле архитектора или конструктора
    /// </summary>
    public NavigatorMepToArIncomingView() {
        InitializeComponent();
        Loaded += NavigatorView_Loaded;
    }

    private void NavigatorView_Loaded(object sender, RoutedEventArgs e) {
        _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[1]);
        _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[2]);
        _dgIncomingTasks.GroupBy(_dgIncomingTasks.Columns[3]);

        _dgIncomingTasks.SortBy(_dgIncomingTasks.Columns[13], DevExpress.Data.ColumnSortOrder.Descending);
        _dgIncomingTasks.SortBy(_dgIncomingTasks.Columns[11]);

        _dgOpeningsReal.GroupBy(_dgOpeningsReal.Columns[1]);
    }

    public override string PluginName => nameof(RevitOpeningPlacement);
    public override string ProjectConfigName => nameof(NavigatorMepToArIncomingView);

    private void SimpleButton_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
