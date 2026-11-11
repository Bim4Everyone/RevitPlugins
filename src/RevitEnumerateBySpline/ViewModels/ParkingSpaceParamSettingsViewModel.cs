using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models.Services;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.ViewModels;

internal class ParkingSpaceParamSettingsViewModel : BaseViewModel {
    
    private ObservableCollection<ParamViewModel>? _dependentParams;
    private ParamViewModel? _selectedDependentParam;
    private ObservableCollection<ParamViewModel>? _dependentSearchParams;
    private ParamViewModel? _selectedDependentSearchParam;
    private string? _searchKey;
    private readonly ParamService _paramService;

    public ParkingSpaceParamSettingsViewModel(ParamService paramService) {
        _paramService = paramService;
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
    
    public string? SearchKey {
        get => _searchKey;
        set => RaiseAndSetIfChanged(ref _searchKey, value);
    }

    public void LoadView(ConfigSettings? settings) {
        DependentParams = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        SelectedDependentParam = DependentParams
            .FirstOrDefault(param => param?.RevitParam?.Name == settings?.DependentParam?.Name)
            ?? DependentParams.FirstOrDefault();
        
        SearchKey = settings?.SearchKey;
        
        DependentSearchParams = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        SelectedDependentSearchParam = DependentSearchParams
            .FirstOrDefault(param => param?.RevitParam?.Name == settings?.DependentSearchParam?.Name) 
            ?? DependentSearchParams.FirstOrDefault();
    } 
    
    // Метод получения коллекции ParamViewModel для DependentParams, DependentSearchParams
    private IEnumerable<ParamViewModel> GetParamViewModels() {
        return _paramService.AllSpatialRevitParams
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            });
    }
}
