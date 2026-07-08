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
    
    private bool _hasError;
    private ObservableCollection<GroupParamViewModel> _groupParamViewModels;
    private GroupParamViewModel _selectedGroupParamViewModel;
    private ObservableCollection<RevitElementGroupViewModel> _viewPlanGroupViewModels;
    
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
    
    public bool HasError {
        get => _hasError;
        set => RaiseAndSetIfChanged(ref _hasError, value);
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
        
                return new RevitElementGroupViewModel {
                    Name = group.Key,
                    RevitElementViewModels = new ObservableCollection<RevitElementViewModel>(viewModels)
                };
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
        if(e.PropertyName == nameof(SelectedGroupParamViewModel)) {
            UpdateGroupParameter();
        }
    }
    
    // Метод обновления видов в зависимости от параметра
    private void UpdateGroupParameter() {
        ViewPlanGroupViewModels = new ObservableCollection<RevitElementGroupViewModel>(GetViewPlanGroupViewModels());
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
        PropertyChanged += OnPropertyChanged;
    }
}
