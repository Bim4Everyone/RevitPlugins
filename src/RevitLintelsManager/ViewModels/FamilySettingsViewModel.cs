using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

internal class FamilySettingsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    
    private ObservableCollection<FamilyViewModel> _lintelFamilyViewModels;
    private FamilyViewModel _selectedLintelFamilyViewModel;
    private ObservableCollection<FamilyViewModel> _openingFamilyViewModels;
    private ObservableCollection<FamilyViewModel> _selectedOpeningFamilyViewModels;

    internal FamilySettingsViewModel(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        LoadView();
    }
    
    
    public ObservableCollection<FamilyViewModel> LintelFamilyViewModels {
        get => _lintelFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _lintelFamilyViewModels, value);
    }
    
    public FamilyViewModel SelectedLintelFamilyViewModel {
        get => _selectedLintelFamilyViewModel;
        set => RaiseAndSetIfChanged(ref _selectedLintelFamilyViewModel, value);
    }
    
    public ObservableCollection<FamilyViewModel> OpeningFamilyViewModels {
        get => _openingFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _openingFamilyViewModels, value);
    }
    
    public ObservableCollection<FamilyViewModel> SelectedOpeningFamilyViewModels {
        get => _selectedOpeningFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _selectedOpeningFamilyViewModels, value);
    }
    
    private IEnumerable<FamilyViewModel> GetLintelFamiliesViewModels() {
        return _revitRepository.LintelFamilies
            .Select(revitFamily => new FamilyViewModel {
                Name = revitFamily.Family.Name,
                Family = revitFamily.Family,
                OrderedParams = revitFamily.OrderedParams
            });
    } 
    
    private IEnumerable<FamilyViewModel> GetOpeningFamiliesViewModels() {
        return _revitRepository.OpeningFamilies
            .Select(revitFamily => new FamilyViewModel {
                Name = revitFamily.Family.Name,
                Family = revitFamily.Family,
                OrderedParams = revitFamily.OrderedParams
            });
    }

    private void OnOpeningFamilyPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not FamilyViewModel vm) {
            return;
        }

        if(e.PropertyName != nameof(vm.IsChecked)) {
            return;
        }

        if(vm.IsChecked) {
            if(!SelectedOpeningFamilyViewModels.Contains(vm)) {
                SelectedOpeningFamilyViewModels.Add(vm);
            }
        } else {
            SelectedOpeningFamilyViewModels.Remove(vm);
        }
    }
    
    private void LoadView() {
        LintelFamilyViewModels = new ObservableCollection<FamilyViewModel>(GetLintelFamiliesViewModels());
        SelectedLintelFamilyViewModel = LintelFamilyViewModels.FirstOrDefault();
        OpeningFamilyViewModels = new ObservableCollection<FamilyViewModel>(GetOpeningFamiliesViewModels());
        SelectedOpeningFamilyViewModels = [];
        foreach(var item in OpeningFamilyViewModels) {
            item.PropertyChanged += OnOpeningFamilyPropertyChanged;
            if(item.IsChecked) {
                SelectedOpeningFamilyViewModels.Add(item);
            }
        }
    }
}
