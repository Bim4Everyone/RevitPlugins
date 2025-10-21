using System.Globalization;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels.FloorHeight;

internal class UserFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
    private string _floorHeight;
    
    public LevelHeightProvider LevelHeightProvider => LevelHeightProvider.UserSettings;

    public string FloorHeight {
        get => _floorHeight;
        set => RaiseAndSetIfChanged(ref _floorHeight, value);
    }

    public bool IsEnabled => true;

    public double? GetFloorHeight() {
        return double.TryParse(FloorHeight, out double result) ? result : null;
    }

    public void LoadConfig(AnnotationsSettings settings) {
        FloorHeight = settings.LevelHeight?.ToString(CultureInfo.CurrentCulture);
    }

    public void SaveConfig(AnnotationsSettings settings) {
        settings.LevelHeight = GetFloorHeight();
    }
}
