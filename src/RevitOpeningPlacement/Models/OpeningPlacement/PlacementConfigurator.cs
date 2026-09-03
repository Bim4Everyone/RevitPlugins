using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Filtration;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers.ClashCheckers;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;

namespace RevitOpeningPlacement.Models.OpeningPlacement;
internal class PlacementConfigurator {
    private readonly RevitRepository _revitRepository;
    private readonly MepCategoryCollection _categories;
    private readonly ILogicalFilterFactory _filterFactory;
    private readonly IFilterContextParser _filterContextParser;
    private readonly List<UnplacedClashModel> _unplacedClashes = [];

    private readonly MepCategoryEnum[] _rectangleMepCategories = [
        MepCategoryEnum.RectangleDuct, MepCategoryEnum.CableTray,
    ];

    private readonly MepCategoryEnum[] _roundMepCategories = [
        MepCategoryEnum.Pipe, MepCategoryEnum.RoundDuct, MepCategoryEnum.Conduit,
    ];

    private readonly FittingCategoryEnum[] _fittingCategories = [
        FittingCategoryEnum.PipeFitting,
        FittingCategoryEnum.CableTrayFitting,
        FittingCategoryEnum.ConduitFitting,
        FittingCategoryEnum.DuctFitting,
    ];

    private readonly Dictionary<MepCategoryEnum, FittingCategoryEnum> _fittingCategoryByMep =
        new() {
            { MepCategoryEnum.Pipe, FittingCategoryEnum.PipeFitting} ,
            { MepCategoryEnum.RoundDuct, FittingCategoryEnum.DuctFitting} ,
            { MepCategoryEnum.RectangleDuct, FittingCategoryEnum.DuctFitting} ,
            { MepCategoryEnum.CableTray, FittingCategoryEnum.CableTrayFitting} ,
            { MepCategoryEnum.Conduit, FittingCategoryEnum.ConduitFitting} ,
        };

    public PlacementConfigurator(
        RevitRepository revitRepository,
        MepCategoryCollection categories,
        ILogicalFilterFactory filterFactory,
        IFilterContextParser filterContextParser) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _filterFactory = filterFactory ?? throw new ArgumentNullException(nameof(filterFactory));
        _filterContextParser = filterContextParser
                               ?? throw new ArgumentNullException(nameof(filterContextParser));
    }

    public IEnumerable<OpeningPlacer> GetPlacersMepOutcomingTasks(ElementId[] mepElementsToFilter) {
        var walls = StructureCategoryEnum.Wall;
        var floors = StructureCategoryEnum.Floor;

        var mepCurveWallClashChecker = ClashChecker.GetMepCurveWallClashChecker(_revitRepository);
        var mepCurveFloorClashChecker = ClashChecker.GetMepCurveFloorClashChecker(_revitRepository);

        List<OpeningPlacer> placers =
        [
            .. GetLinearMepPlacers(
                _roundMepCategories,
                walls,
                mepCurveWallClashChecker,
                new RoundMepWallPlacerInitializer(),
                mepElementsToFilter),
            .. GetLinearMepPlacers(
                _roundMepCategories,
                floors,
                mepCurveFloorClashChecker,
                new RoundMepFloorPlacerInitializer(),
                mepElementsToFilter),
            .. GetLinearMepPlacers(
                _rectangleMepCategories,
                walls,
                mepCurveWallClashChecker,
                new RectangleMepWallPlacerInitializer(),
                mepElementsToFilter),
            .. GetLinearMepPlacers(
                _rectangleMepCategories,
                floors,
                mepCurveFloorClashChecker,
                new RectangleMepFloorPlacerInitializer(),
                mepElementsToFilter),
            .. GetFittingPlacers(
                floors,
                (categories) => ClashChecker.GetFittingFloorClashChecker(_revitRepository, categories),
                new FittingFloorPlacerInitializer(), mepElementsToFilter),
            .. GetFittingPlacers(
                walls,
                (categories) => ClashChecker.GetFittingWallClashChecker(_revitRepository, categories),
                new FittingWallPlacerInitializer(),
                mepElementsToFilter),
        ];
        return placers;
    }

    public List<UnplacedClashModel> GetUnplacedClashes() {
        return _unplacedClashes;
    }

    /// <summary>
    /// Возвращает фильтр по линейным элементам для заданной конфигурации настроек элементов инженерных систем
    /// </summary>
    /// <param name="mepCategory">Настройки фильтрации элементов инженерной системы</param>
    /// <returns>Фильтр по линейным элементам из заданной конфигурации настроек</returns>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public CategoryFilter GetLinearFilter(MepCategory mepCategory) {
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }

        var mepCategoryType = _revitRepository.GetMepCategoryEnum(mepCategory.Name);
        return CreateMepFilter(
            mepCategory.Name,
            FiltersInitializer.GetLinearCategories(mepCategoryType),
            withMinSizes: true,
            mepCategory);
    }

    /// <summary>
    /// Возвращает фильтр по нелинейным элементам для заданной конфигурации настроек элементов инженерных систем
    /// </summary>
    /// <param name="mepCategory">Настройки фильтрации элементов инженерной системы</param>
    /// <returns>Фильтр по нелинейным элементам из заданной конфигурации настроек</returns>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public CategoryFilter GetFittingFilter(MepCategory mepCategory) {
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }

        var mepCategoryType = _revitRepository.GetMepCategoryEnum(mepCategory.Name);
        var fittingCategoryType = _fittingCategoryByMep[mepCategoryType];
        return CreateMepFilter(
            RevitRepository.FittingCategoryNames[fittingCategoryType],
            FiltersInitializer.GetFittingCategories(fittingCategoryType),
            withMinSizes: false,
            mepCategory);
    }

    private List<OpeningPlacer> GetLinearMepPlacers(
        MepCategoryEnum[] mepCategoryTypes,
        StructureCategoryEnum structure,
        IClashChecker structureChecker,
        IMepCurvePlacerInitializer placerInitializer,
        ElementId[] mepElementsToFilter) {

        List<OpeningPlacer> placers = [];
        foreach(var mepCategoryType in mepCategoryTypes) {
            var mepCategory = _categories[mepCategoryType];
            if(mepCategory != null
               && mepCategory.IsSelected
               && IntersectionWithStructureEnabled(
                   mepCategory,
                   RevitRepository.StructureCategoryNames[structure])) {
                // фильтр по категории, минимальным габаритам и критериям, созданным пользователем
                var mepFilter = CreateMepFilter(
                    mepCategory.Name,
                    FiltersInitializer.GetLinearCategories(mepCategoryType),
                    withMinSizes: true,
                    mepCategory);
                placers.AddRange(GetMepPlacers(
                    mepFilter,
                    CreateStructureCategoriesFilter(structure, mepCategory),
                    structureChecker,
                    mepCategory,
                    placerInitializer,
                    mepElementsToFilter));
            }
        }
        return placers;
    }

    private List<OpeningPlacer> GetFittingPlacers(
        StructureCategoryEnum structure,
        Func<MepCategory[], IClashChecker> structureCheckerFunc,
        IFittingPlacerInitializer placerInitializer,
        ElementId[] mepElementsToFilter) {

        List<OpeningPlacer> placers = [];
        foreach(var fittingCategoryType in _fittingCategories) {
            var mepCategories = _categories.GetCategories(fittingCategoryType)
                .Where(category => category.IsSelected)
                .ToArray();
            if(mepCategories.Any(category => IntersectionWithStructureEnabled(
                   category,
                   RevitRepository.StructureCategoryNames[structure]))) {
                // фильтр по категориям соединительных деталей и критериям, созданным пользователем
                var mepFilter = CreateMepFilter(
                    RevitRepository.FittingCategoryNames[fittingCategoryType],
                    FiltersInitializer.GetFittingCategories(fittingCategoryType),
                    withMinSizes: false,
                    mepCategories);
                placers.AddRange(GetFittingPlacers(
                    mepFilter,
                    CreateStructureCategoriesFilter(structure, mepCategories),
                    structureCheckerFunc.Invoke(mepCategories),
                    placerInitializer,
                    mepElementsToFilter,
                    mepCategories));
            }
        }
        return placers;
    }

    private IEnumerable<OpeningPlacer> GetMepPlacers(
        CategoryFilter mepFilter,
        CategoryFilter structureFilter,
        IClashChecker clashChecker,
        MepCategory mepCategory,
        IMepCurvePlacerInitializer placerInitializer,
        ElementId[] mepElements
        ) {

        return GetClashes(mepFilter, structureFilter, clashChecker, mepElements)
            .Select(item => placerInitializer.GetPlacer(_revitRepository, item, mepCategory));
    }

    private IEnumerable<OpeningPlacer> GetFittingPlacers(
        CategoryFilter mepFilter,
        CategoryFilter structureFilter,
        IClashChecker clashChecker,
        IFittingPlacerInitializer placerInitializer,
        ElementId[] mepElements,
        params MepCategory[] mepCategories) {

        return GetClashes(mepFilter, structureFilter, clashChecker, mepElements)
            .Select(item => placerInitializer.GetPlacer(_revitRepository, item, mepCategories));
    }

    private IEnumerable<ClashModel> GetClashes(
        CategoryFilter mepFilter,
        CategoryFilter constructionFilter,
        IClashChecker clashChecker,
        params ElementId[] mepElements) {
        var clashes = ClashInitializer.GetClashes(_revitRepository, mepFilter, constructionFilter, mepElements)
            .ToList();
        if(clashes.Count == 0) {
            return Enumerable.Empty<ClashModel>();
        }

        _unplacedClashes.AddRange(clashes
            .Select(item => new UnplacedClashModel() {
                Message = clashChecker.Check(item),
                Clash = item
            })
            .Where(item => !string.IsNullOrEmpty(item.Message)
               && !item.Message.Equals(RevitRepository.SystemCheck, StringComparison.CurrentCulture)));
        return clashes.Where(item => string.IsNullOrEmpty(clashChecker.Check(item)));
    }

    /// <summary>
    /// Проверяет, включена ли расстановка отверстий в местах пересечений заданной категории элементов инженерных систем с элементами заданной категории конструкций
    /// </summary>
    /// <param name="mepCategory">Настройки расстановки отверстий для категории инженерных элементов</param>
    /// <param name="structureCategoryName">Название категории конструкций</param>
    /// <returns>True, если в настройках расстановки включена проверка на пересечения с заданной категорией конструкций, иначе False</returns>
    private bool IntersectionWithStructureEnabled(MepCategory mepCategory, string structureCategoryName) {
        return mepCategory.Intersections.Any(intersection =>
            intersection.IsSelected
            && intersection.Name.Equals(structureCategoryName));
    }

    /// <summary>
    /// Создает поисковый набор элементов инженерных систем.
    /// Итоговый фильтр формируется как логическая сумма (ИЛИ) логических произведений (И)
    /// правил по минимальным габаритам и фильтра, настроенного пользователем, для каждой категории.
    /// </summary>
    /// <param name="name">Название поискового набора</param>
    /// <param name="categories">Категории элементов, среди которых происходит поиск</param>
    /// <param name="withMinSizes">True, если нужно добавить правила по минимальным габаритам сечения</param>
    /// <param name="mepCategories">Настройки расстановки отверстий для категорий инженерных элементов</param>
    private CategoryFilter CreateMepFilter(
        string name,
        ICollection<BuiltInCategory> categories,
        bool withMinSizes,
        params MepCategory[] mepCategories) {
        var root = _filterFactory.CreateOrFilter();
        foreach(var mepCategory in mepCategories) {
            // правила по габаритам добавляются в отдельный фильтр "И" вместе с фильтром пользователя,
            // т.к. корневой фильтр пользователя может быть настроен как "ИЛИ"
            var categoryFilter = _filterFactory.CreateAndFilter();
            if(withMinSizes) {
                foreach(var minSizeRule in FiltersInitializer.GetMinSizeRules(mepCategory)) {
                    categoryFilter.AddGreaterOrEqualRule(minSizeRule.Param, minSizeRule.Value);
                }
            }

            categoryFilter.AddFilter(ParseUserFilter(mepCategory.MepFilterContext));
            root.AddFilter(categoryFilter);
        }

        return new CategoryFilter(name, root, categories);
    }

    /// <summary>
    /// Создает поисковый набор по запрашиваемым элементам конструкций из настроек категорий инженерных элементов.<br/>
    /// Итоговый фильтр формируется как логическая сумма (ИЛИ) фильтров по конструкциям этой категории
    /// из каждой настройки инженерных элементов.
    /// </summary>
    /// <param name="structureCategory">Запрашиваемая категория конструкций</param>
    /// <param name="mepCategories">Настройки инженерных элементов</param>
    private CategoryFilter CreateStructureCategoriesFilter(
        StructureCategoryEnum structureCategory,
        params MepCategory[] mepCategories) {
        string structureName = RevitRepository.StructureCategoryNames[structureCategory];
        var root = _filterFactory.CreateOrFilter();
        foreach(var mepCategory in mepCategories) {
            root.AddFilter(ParseUserFilter(GetStructureFilterContext(mepCategory, structureName)));
        }

        return new CategoryFilter(
            structureName,
            root,
            [.. _revitRepository.GetCategories(structureCategory).Select(c => c.GetBuiltInCategory())]);
    }

    /// <summary>
    /// Возвращает сериализованный контекст фильтра запрашиваемой категории конструкций
    /// из заданной конфигурации настроек инженерной системы
    /// </summary>
    /// <param name="mepCategory">Конфигурация настроек инженерной системы</param>
    /// <param name="structureName">Название запрашиваемой категории конструкций</param>
    private string GetStructureFilterContext(MepCategory mepCategory, string structureName) {
        return mepCategory.Intersections
            .First(c => c.Name.Equals(structureName, StringComparison.CurrentCultureIgnoreCase))
            .FilterContext;
    }

    /// <summary>
    /// Читает фильтр, настроенный пользователем.
    /// Если фильтр не задан или его не удалось прочитать, возвращает пустой фильтр,
    /// в который проходят все элементы.
    /// <para/>
    /// Метод обязательно вызывать отдельно для каждого родительского фильтра:
    /// один и тот же экземпляр <see cref="ILogicalFilter"/> нельзя добавлять в несколько фильтров.
    /// </summary>
    private ILogicalFilter ParseUserFilter(string filterContext) {
        return _filterContextParser.TryParse(filterContext, out var context)
            ? context!.GetFilter()
            : _filterFactory.CreateAndFilter();
    }
}
