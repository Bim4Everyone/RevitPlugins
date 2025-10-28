using System.Globalization;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels.FloorHeight;

internal class UserFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
    private readonly ILocalizationService _localizationService;
   
    private string _floorHeight;

    public UserFloorHeightViewModel(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

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
            return _localizationService.GetLocalizedString("MainWindow.EmptyFloorHeightValue");
        }
        
        if(!double.TryParse(FloorHeight, out double floorHeight)) {
            return _localizationService.GetLocalizedString("MainWindow.TextFloorHeightValue");
        }

        if(floorHeight < 0) {
            return _localizationService.GetLocalizedString("MainWindow.NegativeFloorHeightValue");
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
