using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Factories;
using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.ViewModels;

internal class RangeViewModel(
    ILocalizationService localizationService,
    ProvidersFactory providersFactory,
    RevitRepository revitRepository)
    : BaseViewModel {
    
    private ObservableCollection<ElementsProviderViewModel>? _range;
    private ElementsProviderViewModel? _selectedRange;
    
    public ObservableCollection<ElementsProviderViewModel>? Range {
        get => _range;
        set => RaiseAndSetIfChanged(ref _range, value);
    }
    
    public ElementsProviderViewModel? SelectedRange {
        get => _selectedRange;
        set => RaiseAndSetIfChanged(ref _selectedRange, value);
    }

    /// <summary>
    /// Метод загрузки окна
    /// </summary>
    /// <param name="selectedRangeElementsProvider"></param>
    public void LoadView() {
        Range = new ObservableCollection<ElementsProviderViewModel>(GetElementsProviderViewModels());
        SelectedRange = ResolveDefaultRange() ?? Range.FirstOrDefault();
    }

    // Метод получения коллекции ElementsProviderViewModel для RangeElements
    private IEnumerable<ElementsProviderViewModel> GetElementsProviderViewModels() {
        var providers = Enum.GetValues(typeof(ElementsProviderType)).Cast<ElementsProviderType>();
        return providers
            .Select(provider => new ElementsProviderViewModel {
                Name = localizationService.GetLocalizedString($"RangeViewModel.{provider}"),
                ElementsProvider = providersFactory.GetElementsProvider(provider)
            });
    }
    
    // Метод получения ElementsProviderViewModel по умолчанию
    private ElementsProviderViewModel? ResolveDefaultRange() {
        return revitRepository.HasSelectedRooms()
            ? Range?.FirstOrDefault(x => x.ElementsProvider?.Type == ElementsProviderType.SelectedElementsProvider)
            : revitRepository.HasRoomsOnCurrentView()
                ? Range?.FirstOrDefault(x => x.ElementsProvider?.Type == ElementsProviderType.CurrentViewProvider)
                : Range?.FirstOrDefault(x => x.ElementsProvider?.Type == ElementsProviderType.AllElementsProvider);
    }
}
