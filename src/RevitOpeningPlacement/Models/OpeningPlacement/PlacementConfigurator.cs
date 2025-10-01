using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers.ClashCheckers;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;

namespace RevitOpeningPlacement.Models.OpeningPlacement;
internal class PlacementConfigurator {
    private readonly RevitRepository _revitRepository;
    private readonly MepCategoryCollection _categories;
    private readonly List<UnplacedClashModel> _unplacedClashes = [];

    private readonly Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter>> _rectangleMepFilterProviders =
        new() {
            { MepCategoryEnum.RectangleDuct, FiltersInitializer.GetRectangleDuctFilter },
            { MepCategoryEnum.CableTray, FiltersInitializer.GetTrayFilter },
        };

    private readonly Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, Filter>> _roundMepFilterProviders =
        new() {
            { MepCategoryEnum.Pipe, FiltersInitializer.GetPipeFilter },
            { MepCategoryEnum.RoundDuct, FiltersInitializer.GetRoundDuctFilter },
            { MepCategoryEnum.Conduit, FiltersInitializer.GetConduitFilter },
        };

    private readonly Dictionary<FittingCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, Filter>> _fittingFilterProviders =
        new() {
            { FittingCategoryEnum.PipeFitting, FiltersInitializer.GetPipeFittingFilter },
            { FittingCategoryEnum.CableTrayFitting, FiltersInitializer.GetTrayFittingFilter },
            { FittingCategoryEnum.ConduitFitting, FiltersInitializer.GetConduitFittingFilter },
            { FittingCategoryEnum.DuctFitting, FiltersInitializer.GetDuctFittingFilter },
        };

    private readonly Dictionary<MepCategoryEnum, FittingCategoryEnum> _fittingCategoryByMep =
        new() {
            { MepCategoryEnum.Pipe, FittingCategoryEnum.PipeFitting} ,
            { MepCategoryEnum.RoundDuct, FittingCategoryEnum.DuctFitting} ,
            { MepCategoryEnum.RectangleDuct, FittingCategoryEnum.DuctFitting} ,
            { MepCategoryEnum.CableTray, FittingCategoryEnum.CableTrayFitting} ,
            { MepCategoryEnum.Conduit, FittingCategoryEnum.ConduitFitting} ,
        };


    public PlacementConfigurator(RevitRepository revitRepository, MepCategoryCollection categories) {
        _revitRepository = revitRepository;
        _categories = categories;
    }

    public IEnumerable<OpeningPlacer> GetPlacersMepOutcomingTasks(ElementId[] mepElementsToFilter) {
        var walls = StructureCategoryEnum.Wall;
        var floors = StructureCategoryEnum.Floor;

        var mepCurveWallClashChecker = ClashChecker.GetMepCurveWallClashChecker(_revitRepository);
        var mepCurveFloorClashChecker = ClashChecker.GetMepCurveFloorClashChecker(_revitRepository);

        List<OpeningPlacer> placers =
        [
            .. GetRoundMepPlacers(walls, mepCurveWallClashChecker, new RoundMepWallPlacerInitializer(), mepElementsToFilter),
            .. GetRoundMepPlacers(floors, mepCurveFloorClashChecker, new RoundMepFloorPlacerInitializer(), mepElementsToFilter),
            .. GetRectangleMepPlacers(walls, mepCurveWallClashChecker, new RectangleMepWallPlacerInitializer(), mepElementsToFilter),
            .. GetRectangleMepPlacers(floors, mepCurveFloorClashChecker, new RectangleMepFloorPlacerInitializer(), mepElementsToFilter),
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
    public Filter GetLinearFilter(MepCategory mepCategory) {
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }

        var mepCategoryType = RevitRepository.MepCategoryNames.First(categoryNamePair => categoryNamePair.Value.Equals(mepCategory.Name)).Key;
        var linearMepStandardFilter = mepCategory.IsRound
            ? GetRoundMepFilter(mepCategoryType, _roundMepFilterProviders[mepCategoryType])
            : GetRectangleMepFilter(mepCategoryType, _rectangleMepFilterProviders[mepCategoryType]);
        var linearMepFilter = CreateMepCategoriesAndFilterSet(_revitRepository.GetClashRevitRepository(), linearMepStandardFilter, mepCategory);

        return linearMepFilter;
    }

    /// <summary>
    /// Возвращает фильтр по нелинейным элементам для заданной конфигурации настроек элементов инженерных систем
    /// </summary>
    /// <param name="mepCategory">Настройки фильтрации элементов инженерной системы</param>
    /// <returns>Фильтр по нелинейным элементам из заданной конфигурации настроек</returns>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public Filter GetFittingFilter(MepCategory mepCategory) {
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }

        var mepCategoryType = RevitRepository.MepCategoryNames.First(categoryNamePair => categoryNamePair.Value.Equals(mepCategory.Name)).Key;
        var fittingCategoryType = _fittingCategoryByMep[mepCategoryType];
        var fittingMepStandardFilter = GetFittingFilter(_fittingFilterProviders[fittingCategoryType]);
        var fittingFilter = CreateMepCategoriesAndFilterSet(_revitRepository.GetClashRevitRepository(), fittingMepStandardFilter, mepCategory);

        return fittingFilter;
    }


    private List<OpeningPlacer> GetRoundMepPlacers(
        StructureCategoryEnum structure,
        IClashChecker structureChecker,
        IMepCurvePlacerInitializer placerInitializer,
        ElementId[] mepElementsToFilter) {

        List<OpeningPlacer> placers = [];
        foreach(var filterProvider in _roundMepFilterProviders) {
            var mepCategory = _categories[filterProvider.Key];
            if(mepCategory.IsSelected && IntersectionWithStructureEnabled(mepCategory, RevitRepository.StructureCategoryNames[structure])) {
                //получение стандартного фильтра по категории и минимальным габаритам
                var mepStandardFilter = GetRoundMepFilter(filterProvider.Key, filterProvider.Value);
                //добавление к стандартному фильтру критериев по параметрам, созданных пользователем
                var mepComplexFilter = CreateMepCategoriesAndFilterSet(
                    _revitRepository.GetClashRevitRepository(),
                    mepStandardFilter,
                    mepCategory);
                placers.AddRange(GetMepPlacers(
                    mepComplexFilter,
                    CreateStructureCategoriesFilter(_revitRepository.GetClashRevitRepository(), structure, mepCategory),
                    structureChecker,
                    _categories[filterProvider.Key],
                    placerInitializer,
                    mepElementsToFilter));
            }
        }
        ;
        return placers;
    }

    private List<OpeningPlacer> GetRectangleMepPlacers(
        StructureCategoryEnum structure,
        IClashChecker structureChecker,
        IMepCurvePlacerInitializer placerInitializer,
        ElementId[] mepElementsToFilter) {

        List<OpeningPlacer> placers = [];
        foreach(var filterProvider in _rectangleMepFilterProviders) {
            var mepCategory = _categories[filterProvider.Key];
            if(mepCategory.IsSelected && IntersectionWithStructureEnabled(mepCategory, RevitRepository.StructureCategoryNames[structure])) {
                //получение стандартного фильтра по категории и минимальным габаритам
                var mepStandardFilter = GetRectangleMepFilter(filterProvider.Key, filterProvider.Value);
                //добавление к стандартному фильтру критериев по параметрам, созданных пользователем
                var mepComplexFilter = CreateMepCategoriesAndFilterSet(
                    _revitRepository.GetClashRevitRepository(),
                    mepStandardFilter,
                    mepCategory);
                placers.AddRange(GetMepPlacers(
                    mepComplexFilter,
                    CreateStructureCategoriesFilter(_revitRepository.GetClashRevitRepository(), structure, mepCategory),
                    structureChecker,
                    _categories[filterProvider.Key],
                    placerInitializer,
                    mepElementsToFilter));
            }
        }
        ;
        return placers;
    }

    private List<OpeningPlacer> GetFittingPlacers(
        StructureCategoryEnum structure,
        Func<MepCategory[], IClashChecker> structureCheckerFunc,
        IFittingPlacerInitializer placerInitializer,
        ElementId[] mepElementsToFilter) {

        List<OpeningPlacer> placers = [];
        foreach(var filterProvider in _fittingFilterProviders) {
            var mepCategories = _categories.GetCategories(filterProvider.Key)
                .Where(category => category.IsSelected)
                .ToArray();
            if(mepCategories.Any(category => IntersectionWithStructureEnabled(category, RevitRepository.StructureCategoryNames[structure]))) {
                //получение стандартного фильтра по категории
                var mepStandardFilter = GetFittingFilter(filterProvider.Value);
                //добавление к стандартному фильтру критериев по параметрам, созданных пользователем
                var mepComplexFilter = CreateMepCategoriesAndFilterSet(
                    _revitRepository.GetClashRevitRepository(),
                    mepStandardFilter,
                    mepCategories);
                placers.AddRange(GetFittingPlacers(
                    mepComplexFilter,
                    CreateStructureCategoriesFilter(_revitRepository.GetClashRevitRepository(), structure, mepCategories),
                    structureCheckerFunc.Invoke(mepCategories),
                    placerInitializer,
                    mepElementsToFilter,
                    mepCategories));
            }
        }
        ;
        return placers;
    }

    private IEnumerable<OpeningPlacer> GetMepPlacers(
        Filter mepFilter,
        Filter structureFilter,
        IClashChecker clashChecker,
        MepCategory mepCategory,
        IMepCurvePlacerInitializer placerInitializer,
        ElementId[] mepElements
        ) {

        return GetClashes(mepFilter, structureFilter, clashChecker, mepElements)
            .Select(item => placerInitializer.GetPlacer(_revitRepository, item, mepCategory));
    }

    private IEnumerable<OpeningPlacer> GetFittingPlacers(
        Filter mepFilter,
        Filter structureFilter,
        IClashChecker clashChecker,
        IFittingPlacerInitializer placerInitializer,
        ElementId[] mepElements,
        params MepCategory[] mepCategories) {

        return GetClashes(mepFilter, structureFilter, clashChecker, mepElements)
            .Select(item => placerInitializer.GetPlacer(_revitRepository, item, mepCategories));
    }

    private IEnumerable<ClashModel> GetClashes(
        Filter mepFilter,
        Filter constructionFilter,
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

    private Filter GetRoundMepFilter(MepCategoryEnum category, Func<RevitClashDetective.Models.RevitRepository, double, Filter> filterProvider) {
        var minSize = _categories[category]?.MinSizes[Parameters.Diameter];
        return minSize != null ? filterProvider.Invoke(_revitRepository.GetClashRevitRepository(), minSize.Value) : null;
    }

    private Filter GetRectangleMepFilter(MepCategoryEnum category, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter> filterProvider) {
        var minSizes = _categories[category]?.MinSizes;
        if(minSizes != null) {
            var height = minSizes[Parameters.Height];
            var width = minSizes[Parameters.Width];
            if(height != null && width != null) {
                return filterProvider.Invoke(_revitRepository.GetClashRevitRepository(), height.Value, width.Value);
            }
        }
        return null;
    }

    private Filter GetFittingFilter(Func<RevitClashDetective.Models.RevitRepository, Filter> filterProvider) {
        return filterProvider.Invoke(_revitRepository.GetClashRevitRepository());
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
    /// Добавляет к заданному фильтру элементов инженерных систем поисковые наборы из настроек категории инженерных элементов.
    /// Итоговый фильтр формируется как логическая сумма (ИЛИ) логических произведений (И) правила фильтрации изначального фильтра и правила фильтрации каждой категории
    /// </summary>
    /// <param name="revitRepository">Репозиторий Revit, в котором будут расставляться отверстия</param>
    /// <param name="mepFilterToAdd">Фильтр инженерных элементов, к которому нужно добавить поисковые наборы из настроек категорий инженерных элементов</param>
    /// <param name="mepCategories">Настройки расстановки отверстий для категорий инженерных элементов</param>
    private Filter CreateMepCategoriesAndFilterSet(RevitClashDetective.Models.RevitRepository revitRepository, Filter mepFilterToAdd, params MepCategory[] mepCategories) {
        var mepCategoriesAndFilterSet = new Set() {
            SetEvaluator = SetEvaluatorUtils.GetEvaluators().First(item => item.Evaluator == SetEvaluators.Or),
            RevitRepository = revitRepository,
            Criteria = []
        };
        foreach(var category in mepCategories) {
            var mepCategoryAndFilterSet = new Set() {
                SetEvaluator = SetEvaluatorUtils.GetEvaluators().First(item => item.Evaluator == SetEvaluators.And),
                RevitRepository = revitRepository,
                Criteria = [
                    mepFilterToAdd.Set,
                    GetMepFilterSet(revitRepository, category)
                ]
            };
            mepCategoriesAndFilterSet.Criteria.Add(mepCategoryAndFilterSet);
        }
        return new Filter(revitRepository) {
            CategoryIds = [.. mepFilterToAdd.CategoryIds],
            Name = mepFilterToAdd.Name,
            Set = mepCategoriesAndFilterSet
        };
    }

    /// <summary>
    /// Возвращает поисковые набор по запрашиваемым элементам конструкций из настроек категории инженерных элементов.<br/>
    /// Итоговый фильтр формируется как логическая сумма (ИЛИ) логических произведений (И) правил фильтрации элементов<br/>
    ///  запрашиваемой категории конструкций по каждой конфигурации настроек инженерных элементов.
    /// </summary>
    /// <param name="revitRepository">Репозиторий Revit, в котором будут расставляться отверстия</param>
    /// <param name="structureCategory">Запрашиваемая категория конструкций</param>
    /// <param name="mepCategories">Настройки инженерных элементов</param>
    /// <returns>Поисковый набор, в который попадают все элементы конструкций заданной категории,
    /// и которые попадают в каждый фильтр по конструкциям этой категории из каждой настройки инженерных элементов</returns>
    private Filter CreateStructureCategoriesFilter(
        RevitClashDetective.Models.RevitRepository revitRepository,
        StructureCategoryEnum structureCategory,
        params MepCategory[] mepCategories) {

        var structureCategoriesSet = new Set() {
            SetEvaluator = SetEvaluatorUtils.GetEvaluators().First(item => item.Evaluator == SetEvaluators.Or),
            RevitRepository = revitRepository,
            Criteria = []
        };
        foreach(var category in mepCategories) {
            var structureCategorySet = new Set() {
                SetEvaluator = SetEvaluatorUtils.GetEvaluators().First(item => item.Evaluator == SetEvaluators.And),
                RevitRepository = revitRepository,
                Criteria = [
                    GetStructureFilterSet(revitRepository, category, structureCategory)
                ]
            };
            structureCategoriesSet.Criteria.Add(structureCategorySet);
        }
        return new Filter(revitRepository) {
            CategoryIds = [.. _revitRepository.GetCategories(structureCategory).Select(c => c.Id)],
            Name = RevitRepository.StructureCategoryNames[structureCategory],
            Set = structureCategoriesSet
        };
    }

    /// <summary>
    /// Возвращает поисковый набор из заданной конфигурации настроек инженерной системы
    /// </summary>
    /// <param name="revitRepository">Репозиторий ревита</param>
    /// <param name="mepCategory">Конфигурация настроек инженерной системы</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private Set GetMepFilterSet(RevitClashDetective.Models.RevitRepository revitRepository, MepCategory mepCategory) {
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }
        if(mepCategory.Set is null) {
            throw new ArgumentNullException(nameof(mepCategory.Set));
        }
        var set = new Set() {
            SetEvaluator = mepCategory.Set.SetEvaluator,
            RevitRepository = revitRepository,
            Criteria = []
        };
        foreach(var criterion in mepCategory.Set.Criteria) {
            criterion.SetRevitRepository(revitRepository);
            set.Criteria.Add(criterion);
        }
        return set;
    }

    /// <summary>
    /// Возвращает поисковый набор запрашиваемой категории конструкций из заданной конфигурации настроек инженерной системы
    /// </summary>
    /// <param name="revitRepository">Репозиторий ревита</param>
    /// <param name="mepCategory">Конфигурация настроек инженерной системы</param>
    /// <param name="structureCategory">Запрашиваемая категория конструкций</param>
    /// <returns>Поисковый набор по конструкциям</returns>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private Set GetStructureFilterSet(
        RevitClashDetective.Models.RevitRepository revitRepository,
        MepCategory mepCategory,
        StructureCategoryEnum structureCategory) {
        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }
        string structureName = RevitRepository.StructureCategoryNames[structureCategory];
        var structure = mepCategory.Intersections
            .First(c => c.Name.Equals(structureName, StringComparison.CurrentCultureIgnoreCase));
        var set = new Set() {
            SetEvaluator = structure.Set.SetEvaluator,
            RevitRepository = revitRepository,
            Criteria = []
        };
        foreach(var criterion in structure.Set.Criteria) {
            criterion.SetRevitRepository(revitRepository);
            set.Criteria.Add(criterion);
        }
        return set;
    }
}
