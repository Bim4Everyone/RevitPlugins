using System;
using System.Linq;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Filtration;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class StructureCategoryViewModel : BaseViewModel {
    public StructureCategoryViewModel(
        RevitRepository revitRepository,
        StructureCategory structureCategory,
        ILocalizationService localizationService,
        ILanguageService languageService,
        ILogicalFilterProviderFactory filterProviderFactory,
        IFilterContextParser filterContextParser,
        ILogicalFilterFactory filterFactory) {
        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }

        if(structureCategory is null) {
            throw new ArgumentNullException(nameof(structureCategory));
        }

        if(filterProviderFactory is null) {
            throw new ArgumentNullException(nameof(filterProviderFactory));
        }

        LocalizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        FilterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));
        if(filterFactory is null) {
            throw new ArgumentNullException(nameof(filterFactory));
        }

        _name = structureCategory.Name;
        _isSelected = structureCategory.IsSelected;
        InitializeFilterProvider(revitRepository, filterProviderFactory, filterFactory, structureCategory);
    }


    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    private string _name;
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Фильтр элементов данной категории конструкций
    /// </summary>
    public ILogicalFilterProvider FilterProvider { get; private set; }

    public ILanguageService LanguageService { get; }

    public ILocalizationService LocalizationService { get; }

    public IFilterContextParser FilterContextParser { get; }

    public StructureCategory GetStructureCategory() {
        return new StructureCategory() {
            Name = Name,
            IsSelected = IsSelected,
            FilterContext = FilterContextParser.Serialize(FilterProvider.GetFilter())
        };
    }

    private void InitializeFilterProvider(
        RevitRepository revitRepository,
        ILogicalFilterProviderFactory filterProviderFactory,
        ILogicalFilterFactory filterFactory,
        StructureCategory structureCategory) {
        var categories = revitRepository.GetCategories(revitRepository.GetStructureCategoryEnum(Name));
        var dataProvider = new FilterDataProvider(categories, [revitRepository.Doc]).CreateDataProvider();
        // если фильтр не задан, создается пустой фильтр по заданным категориям,
        // иначе фильтр останется в состоянии ошибки, пока пользователь не откроет его в интерфейсе
        FilterProvider = FilterContextParser.TryParse(structureCategory.FilterContext, out var context)
            ? filterProviderFactory.Create(dataProvider, context!)
            : filterProviderFactory.Create(
                dataProvider,
                filterFactory.CreateAndFilter(),
                [.. categories.Select(c => c.GetBuiltInCategory())]);
    }
}
