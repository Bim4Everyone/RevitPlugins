using dosymep.WPF.ViewModels;

namespace RevitEnumerateBySpline.ViewModels;

internal class ParamSettingsViewModel : BaseViewModel {
    private CommonParamSettingsViewModel? _commonParamSettingsViewModel;
    private ParkingSpaceParamSettingsViewModel? _parkingSpaceParamSettingsViewModel;

    public CommonParamSettingsViewModel? CommonParamSettingsViewModel {
        get => _commonParamSettingsViewModel;
        set => RaiseAndSetIfChanged(ref _commonParamSettingsViewModel, value);
    }
    public ParkingSpaceParamSettingsViewModel? ParkingSpaceParamSettingsViewModel {
        get => _parkingSpaceParamSettingsViewModel;
        set => RaiseAndSetIfChanged(ref _parkingSpaceParamSettingsViewModel, value);
    }
    
    public void LoadView() {
        CommonParamSettingsViewModel = new CommonParamSettingsViewModel();
        CommonParamSettingsViewModel?.LoadView();

        ParkingSpaceParamSettingsViewModel = new ParkingSpaceParamSettingsViewModel();
        ParkingSpaceParamSettingsViewModel?.LoadView();
    }
    
}
