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

internal class TypeElementSelectionViewModel : BaseViewModel {
    
    private readonly ILocalizationService _localizationService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly AreaBoundarySettings _areaBoundarySettings;
    
    private ObservableCollection<RevitElementGroupViewModel> _typeElementGroupViewModels;
    private ObservableCollection<RevitElementViewModel> _selectedTypeElementViewModels;
    
    public TypeElementSelectionViewModel(
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
    
    
    public ObservableCollection<RevitElementGroupViewModel> TypeElementGroupViewModels {
        get => _typeElementGroupViewModels;
        set => RaiseAndSetIfChanged(ref _typeElementGroupViewModels, value);
    }
    
    public ObservableCollection<RevitElementViewModel> SelectedTypeElementViewModels {
        get => _selectedTypeElementViewModels;
        set => RaiseAndSetIfChanged(ref _selectedTypeElementViewModels, value);
    }

    private IEnumerable<RevitElementGroupViewModel> GetTypeElementGroupViewModels() {
        
        var savedViewIds = _areaBoundarySettings.Types
            .Select(view => view.Element.Id)
            .ToHashSet();

        return _revitRepository.GetTypeModels()
            .GroupBy(type => type.CategoryName)
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
    
    // Метод, подписанный на событие изменения выделенных связанных файлов
    private void OnTypeElementViewModelChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not RevitElementViewModel vm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(vm.IsChecked):
                if(!SelectedTypeElementViewModels.Contains(vm)) {
                    SelectedTypeElementViewModels.Add(vm);
                } else {
                    if(SelectedTypeElementViewModels.Contains(vm)) {
                        SelectedTypeElementViewModels.Remove(vm);
                    }
                }
                break;
        }
    }

    private void LoadView() {
        TypeElementGroupViewModels = new ObservableCollection<RevitElementGroupViewModel>(GetTypeElementGroupViewModels());
        SelectedTypeElementViewModels = [];
        foreach(var group in TypeElementGroupViewModels) {
            foreach(var typeElementViewModel in group.RevitElementViewModels) {
                typeElementViewModel.PropertyChanged += OnTypeElementViewModelChanged;
            }
        }
    }
    
}
