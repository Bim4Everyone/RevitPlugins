using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Services;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.ViewModels;

internal class ParkingSpaceParamSettingsViewModel : BaseViewModel {
    
    private ObservableCollection<ParamViewModel>? _dependentParams;
    private ParamViewModel? _selectedDependentParam;
    private ObservableCollection<ParamViewModel>? _dependentSearchParams;
    private ParamViewModel? _selectedDependentSearchParam;
    private string? _searchKey;
    private readonly IParamService _paramService;
    private readonly RevitRepository _revitRepository;

    public ParkingSpaceParamSettingsViewModel(IParamService paramService, RevitRepository revitRepository) {
        _paramService = paramService;
        _revitRepository = revitRepository;
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

    /// <summary>Загружает параметры машино-мест в форму.</summary>
    public void LoadView(ConfigSettings? settings) {
        DependentParams = new ObservableCollection<ParamViewModel>(GetParamViewModels(writable: true));
        SelectedDependentParam = DependentParams
            .FirstOrDefault(param => param?.RevitParam?.Name == settings?.DependentParam?.Name)
            ?? DependentParams.FirstOrDefault();
        
        SearchKey = settings?.SearchKey;
        
        DependentSearchParams = new ObservableCollection<ParamViewModel>(GetParamViewModels(writable: false));
        SelectedDependentSearchParam = DependentSearchParams
            .FirstOrDefault(param => param?.RevitParam?.Name == settings?.DependentSearchParam?.Name) 
            ?? DependentSearchParams.FirstOrDefault();
    } 
    
    // Получает параметры помещений.
    private IEnumerable<ParamViewModel> GetParamViewModels(bool writable) {
        var spatialElements = _revitRepository.AllSpatialModels;
        return _paramService.AllSpatialRevitParams
            .Where(revitParam => spatialElements.All(spatialModel =>
                _paramService.TryGetParam(revitParam, spatialModel.SpatialElement, out var param)
                && param is { StorageType: StorageType.String }
                && (!writable || !param.IsReadOnly)))
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            });
    }
}
