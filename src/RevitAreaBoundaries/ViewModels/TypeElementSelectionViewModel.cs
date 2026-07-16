using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

internal class TypeElementSelectionViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ConfigSettings _configSettings;
    
    private ObservableCollection<RevitElementGroupViewModel> _typeElementGroupViewModels;
    private ObservableCollection<RevitElementViewModel> _selectedTypeElementViewModels;
    
    public TypeElementSelectionViewModel(
        RevitRepository revitRepository, 
        ConfigSettings configSettings) { 
        
        _revitRepository = revitRepository;
        _configSettings = configSettings;
        
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
        var savedViewIds = _configSettings.Types.ToHashSet();

        return _revitRepository.GetTypeModels()
            .GroupBy(type => type.CategoryName)
            .Select(group => {
                var viewModels = group.Select(type => new RevitElementViewModel {
                    Name = $"{type.FamilyName}: {type.Name}",
                    IsChecked = savedViewIds.Count == 0 || savedViewIds.Contains(type.Element.Id),
                    RevitElement = type
                });
        
                return new RevitElementGroupViewModel (viewModels, group.Key);
            });
        
    }
    
    // Метод, подписанный на событие изменения выделенных типов
    private void OnTypeElementViewModelChanged(object sender, PropertyChangedEventArgs e) {
        if (sender is not RevitElementViewModel vm ||
            e.PropertyName != nameof(RevitElementViewModel.IsChecked)) {
            return;
        }

        if (vm.IsChecked) {
            if (!SelectedTypeElementViewModels.Contains(vm)) {
                SelectedTypeElementViewModels.Add(vm);
            }
        } else {
            SelectedTypeElementViewModels.Remove(vm);
        }
    }

    private void LoadView() {
        TypeElementGroupViewModels = new ObservableCollection<RevitElementGroupViewModel>(GetTypeElementGroupViewModels());
        foreach(var group in TypeElementGroupViewModels) {
            foreach(var typeElementViewModel in group.RevitElementViewModels) {
                typeElementViewModel.PropertyChanged += OnTypeElementViewModelChanged;
            }
        }
        SelectedTypeElementViewModels = new ObservableCollection<RevitElementViewModel>(
            TypeElementGroupViewModels
                .SelectMany(group => group.RevitElementViewModels)
                .Where(vm => vm.IsChecked));
    }
}
