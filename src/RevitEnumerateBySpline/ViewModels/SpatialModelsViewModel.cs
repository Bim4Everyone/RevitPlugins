using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Services;

namespace RevitEnumerateBySpline.ViewModels;

internal class SpatialModelsViewModel : BaseViewModel {
    private readonly ParamService _paramService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private ObservableCollection<ParamViewModel>? _filterParameterViewModels;
    private ParamViewModel? _selectedFilterParameterViewModel;
    private ObservableCollection<SpatialModelViewModel>? _spatialModelViewModels;
    private string _searchText = string.Empty;
    private ObservableCollection<SpatialModelViewModel>? _filteredSpatialModelViewModels;
    private bool _isNotSelected = true;

    public SpatialModelsViewModel(ParamService paramService, SystemPluginConfig systemPluginConfig, IMessenger messenger) {
        _paramService = paramService;
        _systemPluginConfig = systemPluginConfig;

        ResetFilterCommand = RelayCommand.Create(ResetFilter);
        SearchCommand = RelayCommand.Create(ApplySearch);
        SelectAllCommand = RelayCommand.Create(SelectAll);
        ClearSelectionsCommand = RelayCommand.Create(ClearSelections);

        PropertyChanged += OnPropertyChanged;
        
        messenger.Subscribe<RangeChangedMessage>(this, OnRangeChanged);

        LoadFilterParameterViewModels();
    }

    public ICommand ResetFilterCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand ClearSelectionsCommand { get; }
    
    public ObservableCollection<ParamViewModel>? FilterParameterViewModels {
        get => _filterParameterViewModels;
        set => RaiseAndSetIfChanged(ref _filterParameterViewModels, value);
    }
    
    public ParamViewModel? SelectedFilterParameterViewModel {
        get => _selectedFilterParameterViewModel;
        set => RaiseAndSetIfChanged(ref _selectedFilterParameterViewModel, value);
    }
    
    public ObservableCollection<SpatialModelViewModel>? SpatialModelViewModels {
        get => _spatialModelViewModels;
        set => RaiseAndSetIfChanged(ref _spatialModelViewModels, value);
    }
    
    public ObservableCollection<SpatialModelViewModel>? FilteredSpatialModelViewModels {
        get => _filteredSpatialModelViewModels;
        set => RaiseAndSetIfChanged(ref _filteredSpatialModelViewModels, value);
    }
    
    public string SearchText {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }
    
    public bool IsNotSelected {
        get => _isNotSelected;
        set => RaiseAndSetIfChanged(ref _isNotSelected, value);
    }

    // Метод сброса фильтра на значение по умолчанию
    private void ResetFilter() {
        SelectedFilterParameterViewModel = FilterParameterViewModels?
            .FirstOrDefault(param => param?.RevitParam?.Id == _paramService.DefaultSpatialFilterParam?.Id) 
            ?? FilterParameterViewModels?.FirstOrDefault();
    }
    
    // Метод команды на выделение всех помещений
    private void SelectAll() {
        if(FilteredSpatialModelViewModels == null) {
            return;
        }
        foreach(var vm in FilteredSpatialModelViewModels) {
            vm.IsChecked = true;
        }
    }

    // Метод команды на снятие выделения всех помещений
    private void ClearSelections() {
        if(FilteredSpatialModelViewModels == null) {
            return;
        }
        foreach(var vm in FilteredSpatialModelViewModels) {
            vm.IsChecked = false;
        }
    }
    
    // Метод для реализации поиска в списке помещений
    private void ApplySearch() {
        if(SpatialModelViewModels != null) {
            FilteredSpatialModelViewModels = string.IsNullOrEmpty(SearchText)
                ? new ObservableCollection<SpatialModelViewModel>(SpatialModelViewModels)
                : new ObservableCollection<SpatialModelViewModel>(SpatialModelViewModels?
                    .Where(item => item.Name
                        .IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ?? []);
        }
    }
    // Метод создания FilterParameterViewModels
    private void LoadFilterParameterViewModels() {
        FilterParameterViewModels = new ObservableCollection<ParamViewModel>(GetFilterParamViewModels());
        SelectedFilterParameterViewModel = FilterParameterViewModels
            .FirstOrDefault(param => param?.RevitParam?.Id == _paramService.DefaultSpatialFilterParam?.Id)
            ?? FilterParameterViewModels.FirstOrDefault();
    }
    
    // Метод получения коллекции ParamViewModel для FilterParameterViewModels
    private IEnumerable<ParamViewModel> GetFilterParamViewModels() {
        return _paramService.AllSpatialRevitParams
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            });
    }
        
    // Метод, подписанный на сообщение смены диапазона помещений
    private void OnRangeChanged(RangeChangedMessage message) {
        var provider = message.Provider;
        LoadSpatialModelViewModels(provider);
        UpdateSpatialViewModelNames();
    }
    
    // Метод создания SpatialModelViewModels из помещений
    private void LoadSpatialModelViewModels(IElementsProvider? elementsProvider) {
        SpatialModelViewModels = new ObservableCollection<SpatialModelViewModel>(GetSpatialModelViewModels(elementsProvider));
        FilteredSpatialModelViewModels = new ObservableCollection<SpatialModelViewModel>(SpatialModelViewModels);
    } 
    
    // Метод получения коллекции SpatialModelViewModel для SpatialModelViewModels
    private IEnumerable<SpatialModelViewModel> GetSpatialModelViewModels(IElementsProvider? elementsProvider) {
        var spatialModels = elementsProvider?.GetSpatialElements();
        if(spatialModels?.Count == 0) {
            return [];
        }
        var filterParam = SelectedFilterParameterViewModel?.RevitParam;
        return spatialModels?.Select(model => new SpatialModelViewModel {
            Name = _paramService.GetParamValue(model, filterParam),
            IsChecked = false,
            SpatialModel = model
        }) ?? [];
    }
    
    // Метод, подписанный на изменение параметра фильтрации
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(SelectedFilterParameterViewModel)) {
            return;
        }
        UpdateSpatialViewModelNames();
    }
    
    // Метод обновления значений помещений
    private void UpdateSpatialViewModelNames() {
        if(SpatialModelViewModels == null) {
            return;
        }
        foreach(var spatialModelViewModel in SpatialModelViewModels) {
            if(spatialModelViewModel.SpatialModel is null) {
                continue;
            }
            spatialModelViewModel.Name = _paramService.GetParamValue(
                spatialModelViewModel.SpatialModel, 
                SelectedFilterParameterViewModel?.RevitParam);
        }
    }
}
