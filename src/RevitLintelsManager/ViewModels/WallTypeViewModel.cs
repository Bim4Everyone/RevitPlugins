using Autodesk.Revit.DB;

namespace RevitLintelsManager.ViewModels;

internal class WallTypeViewModel : ItemCheckViewModel {
    private bool _isAvailable = true;

    public WallType WallType { get; set; }

    public bool IsAvailable {
        get => _isAvailable;
        set => RaiseAndSetIfChanged(ref _isAvailable, value);
    }
}
