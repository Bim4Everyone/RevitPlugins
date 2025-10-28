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

    public string GetErrorText() {
        if(string.IsNullOrEmpty(FloorHeight)) {
            return "Высота типового этажа должна быть заполнена.";
        }
        
        if(!double.TryParse(FloorHeight, out double floorHeight)) {
            return "Высота типового этажа должна быть числом.";
        }

        if(floorHeight < 0) {
            return "Высота типового этажа должна быть неотрицательной.";
        }

        return null;
    }

    public void LoadConfig(RevitSettings settings) {
        FloorHeight = settings?.LevelHeight?.ToString(CultureInfo.CurrentCulture);
    }

    public void SaveConfig(RevitSettings settings) {
        settings.LevelHeight = GetFloorHeight();
    }
}
