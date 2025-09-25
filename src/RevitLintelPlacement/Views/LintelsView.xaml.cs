using System.Linq;

namespace RevitLintelPlacement.Views;

/// <summary>
///     Логика взаимодействия для LintelsTabView.xaml
/// </summary>
public partial class LintelsView {
    public LintelsView() {
        InitializeComponent();
        _gridControl.GroupBy(_gridControl.Columns.Last());
    }

    public override string PluginName => nameof(RevitLintelPlacement);
    public override string ProjectConfigName => nameof(LintelsView);
}
