using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

internal class CommonSettingsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly ConfigSettings _configSettings;
    
    private ObservableCollection<AlgorithmTypeViewModel> _algorithmTypeViewModels;
    private AlgorithmTypeViewModel _selectedAlgorithmTypeViewModel;
    private string _sectionHeight;
    
    
    public CommonSettingsViewModel(ILocalizationService localizationService, ConfigSettings configSettings) {
        _localizationService = localizationService;
        _configSettings = configSettings;
        
        LoadView();
    }
    
    public ObservableCollection<AlgorithmTypeViewModel> AlgorithmTypeViewModels {
        get => _algorithmTypeViewModels;
        set => RaiseAndSetIfChanged(ref _algorithmTypeViewModels, value);
    }
    
    public string SectionHeight {
        get => _sectionHeight;
        set => RaiseAndSetIfChanged(ref _sectionHeight, value);
    }
    
    public AlgorithmTypeViewModel SelectedAlgorithmTypeViewModel {
        get => _selectedAlgorithmTypeViewModel;
        set => RaiseAndSetIfChanged(ref _selectedAlgorithmTypeViewModel, value);
    }

    private IEnumerable<AlgorithmTypeViewModel> GetAlgorithmTypeViewModels() {
        var currentAlgorithmType = _configSettings.AlgorithmType;
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
        SectionHeight = _configSettings.SectionHeight.ToString(CultureInfo.InvariantCulture);
    }
}
