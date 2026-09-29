using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitEnumerateBySpline.ViewModels;

internal class ParkingSpaceParamSettingsViewModel : BaseViewModel {
    
    private ObservableCollection<ParamViewModel>? _dependentParams;
    private ParamViewModel? _selectedDependentParam;
    private ObservableCollection<ParamViewModel>? _dependentSearchParams;
    private ParamViewModel? _selectedDependentSearchParam;
    private string _searchKey = string.Empty;
    private string _prefix = string.Empty;
    private string _suffix = string.Empty;
    
    public ParkingSpaceParamSettingsViewModel() {
        LoadView();
    }

    public ObservableCollection<ParamViewModel>? DependentParams {
        get => _dependentParams;
        set => RaiseAndSetIfChanged(ref _dependentParams, value);
    }
    
    public ParamViewModel? SelectedDependentParam {
        get => _selectedDependentParam;
        set => RaiseAndSetIfChanged(ref _selectedDependentParam, value);
    }
    
    public ObservableCollection<ParamViewModel>? DependentSearchParams {
        get => _dependentSearchParams;
        set => RaiseAndSetIfChanged(ref _dependentSearchParams, value);
    }
    
    public ParamViewModel? SelectedDependentSearchParam {
        get => _selectedDependentSearchParam;
        set => RaiseAndSetIfChanged(ref _selectedDependentSearchParam, value);
    }
    
    public string SearchKey {
        get => _searchKey;
        set => RaiseAndSetIfChanged(ref _searchKey, value);
    }

    private void LoadView() {
       
    } 
    
}
