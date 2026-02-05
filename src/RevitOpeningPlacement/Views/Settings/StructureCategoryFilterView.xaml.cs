namespace RevitOpeningPlacement.Views.Settings;
public partial class StructureCategoryFilterView {
    public StructureCategoryFilterView() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitOpeningPlacement);

    public override string ProjectConfigName => nameof(StructureCategoryFilterView);
}
