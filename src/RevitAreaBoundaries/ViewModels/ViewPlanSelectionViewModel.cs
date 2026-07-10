using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

internal class ViewPlanSelectionViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly AreaBoundarySettings _areaBoundarySettings;
    
    private ObservableCollection<GroupParamViewModel> _groupParamViewModels;
    private GroupParamViewModel _selectedGroupParamViewModel;
    private ObservableCollection<RevitElementGroupViewModel> _viewPlanGroupViewModels;
    private ObservableCollection<RevitElementViewModel> _selectedViewPlanViewModels;
    
    public ViewPlanSelectionViewModel(
        ILocalizationService localizationService, 
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository, 
        AreaBoundarySettings areaBoundarySettings) { 
        _localizationService = localizationService;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _areaBoundarySettings = areaBoundarySettings;
        
        LoadView();
    }
    
    public ObservableCollection<GroupParamViewModel> GroupParamViewModels {
        get => _groupParamViewModels;
        set => RaiseAndSetIfChanged(ref _groupParamViewModels, value);
    }
    
    public GroupParamViewModel SelectedGroupParamViewModel {
        get => _selectedGroupParamViewModel;
        set => RaiseAndSetIfChanged(ref _selectedGroupParamViewModel, value);
    }
    
    public ObservableCollection<RevitElementGroupViewModel> ViewPlanGroupViewModels {
        get => _viewPlanGroupViewModels;
        set => RaiseAndSetIfChanged(ref _viewPlanGroupViewModels, value);
    }
    
    public ObservableCollection<RevitElementViewModel> SelectedViewPlanViewModels {
        get => _selectedViewPlanViewModels;
        set => RaiseAndSetIfChanged(ref _selectedViewPlanViewModels, value);
    }

    private IEnumerable<RevitElementGroupViewModel> GetViewPlanGroupViewModels() {
        var savedViewIds = _areaBoundarySettings.Views
            .Select(view => view.Element.Id)
            .ToHashSet();

        var groupParam = SelectedGroupParamViewModel?.Parameter;

        return _revitRepository.GetViewPlans()
            .GroupBy(viewPlan => _revitRepository.GetGroupNameViewPlan(viewPlan.Element, groupParam))
            .Select(group => {
                var viewModels = group.Select(view => new RevitElementViewModel {
                    Name = view.Name,
                    IsChecked = savedViewIds.Contains(view.Element.Id),
                    RevitElement = view
                });
        
                return new RevitElementGroupViewModel (viewModels, group.Key);
            });
        
    }

    private IEnumerable<GroupParamViewModel> GetGroupParamViewModels() {
        var view = _revitRepository.ViewPlans.FirstOrDefault();
        if(view is null) {
            return [new GroupParamViewModel {
                Parameter = null,
                Name = _localizationService.GetLocalizedString("ViewPlanSelectionViewModel.NoViews")
            }];
        }

        var browserParameters = _revitRepository.GetBrowserParameters(view).ToList();
        if(browserParameters.Count == 0) {
            return [new GroupParamViewModel {
                Parameter = null,
                Name = _localizationService.GetLocalizedString("ViewPlanSelectionViewModel.NoBrowserParameter")
            }];
        }
        
        return browserParameters
            .Select(param => new GroupParamViewModel {
                    Parameter = param,
                    Name = param.Name
            });
    }
    
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(SelectedGroupParamViewModel):
                UpdateGroupParameter();
                break;
        }
    }
    
    // Метод обновления видов в зависимости от параметра
    private void UpdateGroupParameter() {
        ViewPlanGroupViewModels = new ObservableCollection<RevitElementGroupViewModel>(GetViewPlanGroupViewModels());
    }
    
    // Метод, подписанный на событие изменения выделенных связанных файлов
    private void OnViewViewModelChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not RevitElementViewModel vm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(vm.IsChecked):
                if(!SelectedViewPlanViewModels.Contains(vm)) {
                    SelectedViewPlanViewModels.Add(vm);
                } else {
                    if(SelectedViewPlanViewModels.Contains(vm)) {
                        SelectedViewPlanViewModels.Remove(vm);
                    }
                }
                break;
        }
    }

    private void LoadView() {
        GroupParamViewModels = new ObservableCollection<GroupParamViewModel>(GetGroupParamViewModels());
        SelectedGroupParamViewModel =
            GroupParamViewModels.FirstOrDefault(vm =>
                string.Equals(vm.Name, _areaBoundarySettings.GroupParam, StringComparison.OrdinalIgnoreCase))
            ?? GroupParamViewModels.FirstOrDefault(vm =>
                string.Equals(vm.Name, _systemPluginConfig.DefaultGroupParamName, StringComparison.OrdinalIgnoreCase))
            ?? GroupParamViewModels.FirstOrDefault();
        ViewPlanGroupViewModels = new ObservableCollection<RevitElementGroupViewModel>(GetViewPlanGroupViewModels());
        SelectedViewPlanViewModels = [];
        foreach(var group in ViewPlanGroupViewModels) {
            foreach(var viewViewModel in group.RevitElementViewModels) {
                viewViewModel.PropertyChanged += OnViewViewModelChanged;
            }
        }
        PropertyChanged += OnPropertyChanged;
    }
}
