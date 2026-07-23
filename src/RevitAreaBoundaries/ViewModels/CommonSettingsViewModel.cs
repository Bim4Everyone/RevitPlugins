using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Models.Processors;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

internal class CommonSettingsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly ConfigSettings _configSettings;
    private readonly IEnumerable<IBoundaryDrawer> _boundaryDrawers;
    
    private ObservableCollection<DrawerTypeViewModel> _drawerTypeViewModels;
    private DrawerTypeViewModel _selectedDrawerTypeViewModel;
    private string _sectionHeight;
    
    public CommonSettingsViewModel(
        ILocalizationService localizationService, 
        ConfigSettings configSettings, 
        IEnumerable<IBoundaryDrawer> boundaryDrawers) {
        _localizationService = localizationService;
        _configSettings = configSettings;
        _boundaryDrawers = boundaryDrawers;
        LoadView();
    }
    
    public ObservableCollection<DrawerTypeViewModel> DrawerTypeViewModels {
        get => _drawerTypeViewModels;
        set => RaiseAndSetIfChanged(ref _drawerTypeViewModels, value);
    }
    
    public string SectionHeight {
        get => _sectionHeight;
        set => RaiseAndSetIfChanged(ref _sectionHeight, value);
    }
    
    public DrawerTypeViewModel SelectedDrawerTypeViewModel {
        get => _selectedDrawerTypeViewModel;
        set => RaiseAndSetIfChanged(ref _selectedDrawerTypeViewModel, value);
    }
    
    private IEnumerable<DrawerTypeViewModel> GetAlgorithmTypeViewModels() {
        var currentDrawerType = _configSettings.DrawerType;

        return _boundaryDrawers
            .Select(bd => new DrawerTypeViewModel {
                BoundaryDrawer = bd,
                Name = _localizationService.GetLocalizedString($"CommonSettingViewModel.{bd.DrawerType}")
            })
            .OrderByDescending(vm => vm.BoundaryDrawer.DrawerType == currentDrawerType)
            .ThenBy(vm => vm.Name);
    }
    
    private void LoadView() {
        DrawerTypeViewModels = new ObservableCollection<DrawerTypeViewModel>(GetAlgorithmTypeViewModels());
        SelectedDrawerTypeViewModel = DrawerTypeViewModels.FirstOrDefault();
        SectionHeight = _configSettings.SectionHeightMm.ToString(CultureInfo.InvariantCulture);
    }
}
