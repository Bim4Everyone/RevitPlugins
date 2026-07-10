using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

internal class CommonSettingsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly AreaBoundarySettings _areaBoundarySettings;
    
    private ObservableCollection<AlgorithmTypeViewModel> _algorithmTypeViewModels;
    private AlgorithmTypeViewModel _selectedAlgorithmTypeViewModel;
    private double _sectionHeight;
    
    
    public CommonSettingsViewModel(ILocalizationService localizationService, AreaBoundarySettings areaBoundarySettings) {
        _localizationService = localizationService;
        _areaBoundarySettings = areaBoundarySettings;
        
        LoadView();
    }
    
    public ObservableCollection<AlgorithmTypeViewModel> AlgorithmTypeViewModels {
        get => _algorithmTypeViewModels;
        set => RaiseAndSetIfChanged(ref _algorithmTypeViewModels, value);
    }
    
    public double SectionHeight {
        get => _sectionHeight;
        set => RaiseAndSetIfChanged(ref _sectionHeight, value);
    }
    
    public AlgorithmTypeViewModel SelectedAlgorithmTypeViewModel {
        get => _selectedAlgorithmTypeViewModel;
        set => RaiseAndSetIfChanged(ref _selectedAlgorithmTypeViewModel, value);
    }

    private IEnumerable<AlgorithmTypeViewModel> GetAlgorithmTypeViewModels() {
        var currentAlgorithmType = _areaBoundarySettings.AlgorithmType;
        var algorithmTypes = Enum.GetValues(typeof(AlgorithmType)).Cast<AlgorithmType>();
        return algorithmTypes
            .Select(algorithmType => new AlgorithmTypeViewModel {
                AlgorithmType = algorithmType,
                Name = _localizationService.GetLocalizedString($"CommonSettingViewModel.{algorithmType}")
            })
            .OrderByDescending(vm => vm.AlgorithmType == currentAlgorithmType)
            .ThenBy(vm => vm.Name);
    }
    
    private void LoadView() {
        AlgorithmTypeViewModels = new ObservableCollection<AlgorithmTypeViewModel>(GetAlgorithmTypeViewModels());
        SelectedAlgorithmTypeViewModel = AlgorithmTypeViewModels.FirstOrDefault();
        SectionHeight = _areaBoundarySettings.SectionHeight;
    }
}
