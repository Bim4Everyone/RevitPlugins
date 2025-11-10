namespace RevitOpeningPlacement.Views.Utils;
public partial class MepCategoryFilterView {
    public MepCategoryFilterView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitOpeningPlacement);

    public override string ProjectConfigName => nameof(MepCategoryFilterView);
}
