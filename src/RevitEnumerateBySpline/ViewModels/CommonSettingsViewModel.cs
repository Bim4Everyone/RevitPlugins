using System.ComponentModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Factories;
using RevitEnumerateBySpline.Models.Services;

namespace RevitEnumerateBySpline.ViewModels;

internal class CommonSettingsViewModel(
    ILocalizationService localizationService,
    ParamService paramService,
    ProvidersFactory providersFactory,
    RevitRepository revitRepository,
    SystemPluginConfig systemPluginConfig)
    : BaseViewModel {
    
    private RangeViewModel? _rangeViewModel;
    private SpatialModelsViewModel? _spatialModelsViewModel;
    
    public RangeViewModel? RangeViewModel {
        get => _rangeViewModel;
        set => RaiseAndSetIfChanged(ref _rangeViewModel, value);
    }
    
    public SpatialModelsViewModel? SpatialModelsViewModel {
        get => _spatialModelsViewModel;
        set => RaiseAndSetIfChanged(ref _spatialModelsViewModel, value);
    }

    /// <summary>
    /// Метод загрузки окна
    /// </summary>
    public void LoadView() {
        RangeViewModel = new RangeViewModel(localizationService, providersFactory, revitRepository);
        RangeViewModel?.LoadView();

        if(RangeViewModel?.SelectedRange is null) {
            return;
        }
        RangeViewModel.PropertyChanged += OnRangeViewModelChanged;

        SpatialModelsViewModel = new SpatialModelsViewModel(paramService, systemPluginConfig);
        SpatialModelsViewModel?.LoadFilterParameterViewModels();
        SpatialModelsViewModel?.LoadSpatialModelViewModels(RangeViewModel?.SelectedRange?.ElementsProvider);
    }

    private void OnRangeViewModelChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(RangeViewModel.SelectedRange)) {
            return;
        }
        SpatialModelsViewModel?.LoadSpatialModelViewModels(RangeViewModel?.SelectedRange?.ElementsProvider);
        SpatialModelsViewModel?.UpdateSpatialViewModelNames();
    }
}
