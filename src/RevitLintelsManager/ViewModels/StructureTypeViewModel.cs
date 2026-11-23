using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.ViewModels;

internal class StructureTypeViewModel(StructureTypeSelection selection) : ItemCheckViewModel {
    public StructureTypeSelection Selection { get; } = selection;
    public bool IsAvailable => Selection.Resolution == StructureTypeResolution.Resolved;
}
