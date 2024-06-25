using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class PlacementConfigurator {
        private readonly RevitRepository _revitRepository;
        private readonly MepCategoryCollection _categories;
        private readonly List<UnplacedClashModel> _unplacedClashes = new List<UnplacedClashModel>();

        private readonly Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter>> _rectangleMepFilterProviders =
            new Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter>> {
                { MepCategoryEnum.RectangleDuct, (revitRepository, height, width) => { return FiltersInitializer.GetRectangleDuctFilter(revitRepository, height, width); } },
                { MepCategoryEnum.CableTray, (revitRepository, height, width) => { return FiltersInitializer.GetTrayFilter(revitRepository, height, width); } },
            };

        private readonly Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, Filter>> _roundMepFilterProviders =
            new Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, Filter>> {
                { MepCategoryEnum.Pipe, (revitRepository, diameter) => { return FiltersInitializer.GetPipeFilter(revitRepository, diameter); } },
                { MepCategoryEnum.RoundDuct, (revitRepository, diameter) => { return FiltersInitializer.GetRoundDuctFilter(revitRepository, diameter); } },
                { MepCategoryEnum.Conduit, (revitRepository, diameter) => { return FiltersInitializer.GetConduitFilter(revitRepository, diameter); } },
            };

        private readonly Dictionary<FittingCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, Filter>> _fittingFilterProviders =
            new Dictionary<FittingCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, Filter>> {
                { FittingCategoryEnum.PipeFitting, (revitRepository) => { return FiltersInitializer.GetPipeFittingFilter(revitRepository); } },
                { FittingCategoryEnum.CableTrayFitting, (revitRepository) => { return FiltersInitializer.GetTrayFittingFilter(revitRepository); } },
                { FittingCategoryEnum.ConduitFitting, (revitRepository) => { return FiltersInitializer.GetConduitFittingFilter(revitRepository); } },
                { FittingCategoryEnum.DuctFitting, (revitRepository) => { return FiltersInitializer.GetDuctFittingFilter(revitRepository); } },
            };

        private readonly Dictionary<MepCategoryEnum, FittingCategoryEnum> _fittingCategoryByMep =
            new Dictionary<MepCategoryEnum, FittingCategoryEnum> {
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
            var wallFilter = FiltersInitializer.GetWallFilter(_revitRepository.GetClashRevitRepository());
            var floorFilter = FiltersInitializer.GetFloorFilter(_revitRepository.GetClashRevitRepository());

            var mepCurveWallClashChecker = ClashChecker.GetMepCurveWallClashChecker(_revitRepository);
            var mepCurveFloorClashChecker = ClashChecker.GetMepCurveFloorClashChecker(_revitRepository);

            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            placers.AddRange(GetRoundMepPlacers(wallFilter, mepCurveWallClashChecker, new RoundMepWallPlacerInitializer(), mepElementsToFilter));
            placers.AddRange(GetRoundMepPlacers(floorFilter, mepCurveFloorClashChecker, new RoundMepFloorPlacerInitializer(), mepElementsToFilter));
            placers.AddRange(GetRectangleMepPlacers(wallFilter, mepCurveWallClashChecker, new RectangleMepWallPlacerInitializer(), mepElementsToFilter));
            placers.AddRange(GetRectangleMepPlacers(floorFilter, mepCurveFloorClashChecker, new RectangleMepFloorPlacerInitializer(), mepElementsToFilter));
            placers.AddRange(GetFittingPlacers(
                floorFilter,
                (categories) => ClashChecker.GetFittingFloorClashChecker(_revitRepository, categories),
                new FittingFloorPlacerInitializer(), mepElementsToFilter));
            placers.AddRange(GetFittingPlacers(
                wallFilter,
                (categories) => ClashChecker.GetFittingWallClashChecker(_revitRepository, categories),
                new FittingWallPlacerInitializer(),
                mepElementsToFilter));
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
        /// <exception cref="ArgumentNullException"></exception>
        public Filter GetLinearFilter(MepCategory mepCategory) {
            if(mepCategory is null) {
                throw new ArgumentNullException(nameof(mepCategory));
            }

            MepCategoryEnum mepCategoryType = RevitRepository.MepCategoryNames.First(categoryNamePair => categoryNamePair.Value.Equals(mepCategory.Name)).Key;
            Filter linearMepStandardFilter;
            if(mepCategory.IsRound) {
                linearMepStandardFilter = GetRoundMepFilter(mepCategoryType, _roundMepFilterProviders[mepCategoryType]);
            } else {
                linearMepStandardFilter = GetRectangleMepFilter(mepCategoryType, _rectangleMepFilterProviders[mepCategoryType]);
            }
            Filter linearMepFilter = CreateMepCategoriesAndFilterSet(_revitRepository.GetClashRevitRepository(), linearMepStandardFilter, mepCategory);

            return linearMepFilter;
        }

        /// <summary>
        /// Возвращает фильтр по нелинейным элементам для заданной конфигурации настроек элементов инженерных систем
        /// </summary>
        /// <param name="mepCategory">Настройки фильтрации элементов инженерной системы</param>
        /// <returns>Фильтр по нелинейным элементам из заданной конфигурации настроек</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Filter GetFittingFilter(MepCategory mepCategory) {
            if(mepCategory is null) {
                throw new ArgumentNullException(nameof(mepCategory));
            }

            MepCategoryEnum mepCategoryType = RevitRepository.MepCategoryNames.First(categoryNamePair => categoryNamePair.Value.Equals(mepCategory.Name)).Key;
            FittingCategoryEnum fittingCategoryType = _fittingCategoryByMep[mepCategoryType];
            Filter fittingMepStandardFilter = GetFittingFilter(_fittingFilterProviders[fittingCategoryType]);
            Filter fittingFilter = CreateMepCategoriesAndFilterSet(_revitRepository.GetClashRevitRepository(), fittingMepStandardFilter, mepCategory);

            return fittingFilter;
        }


        private List<OpeningPlacer> GetRoundMepPlacers(
            Filter structureFilter,
            IClashChecker structureChecker,
            IMepCurvePlacerInitializer placerInitializer,
            ElementId[] mepElementsToFilter) {

            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            foreach(var filterProvider in _roundMepFilterProviders) {
                MepCategory mepCategory = _categories[filterProvider.Key];
                if(mepCategory.IsSelected && IntersectionWithStructureEnabled(mepCategory, structureFilter.Name)) {
                    //получение стандартного фильтра по категории и минимальным габаритам
                    Filter mepStandardFilter = GetRoundMepFilter(filterProvider.Key, filterProvider.Value);
                    //добавление к стандартному фильтру критериев по параметрам, созданных пользователем
                    Filter mepComplexFilter = CreateMepCategoriesAndFilterSet(
                        _revitRepository.GetClashRevitRepository(),
                        mepStandardFilter,
                        mepCategory);
                    placers.AddRange(GetMepPlacers(
                        mepComplexFilter,
                        structureFilter,
                        structureChecker,
                        _categories[filterProvider.Key],
                        placerInitializer,
                        mepElementsToFilter));
                }
            };
            return placers;
        }

        private List<OpeningPlacer> GetRectangleMepPlacers(
            Filter structureFilter,
            IClashChecker structureChecker,
            IMepCurvePlacerInitializer placerInitializer,
            ElementId[] mepElementsToFilter) {

            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            foreach(var filterProvider in _rectangleMepFilterProviders) {
                MepCategory mepCategory = _categories[filterProvider.Key];
                if(mepCategory.IsSelected && IntersectionWithStructureEnabled(mepCategory, structureFilter.Name)) {
                    //получение стандартного фильтра по категории и минимальным габаритам
                    Filter mepStandardFilter = GetRectangleMepFilter(filterProvider.Key, filterProvider.Value);
                    //добавление к стандартному фильтру критериев по параметрам, созданных пользователем
                    Filter mepComplexFilter = CreateMepCategoriesAndFilterSet(
                        _revitRepository.GetClashRevitRepository(),

                        mepStandardFilter, mepCategory);
                    placers.AddRange(GetMepPlacers(
                        mepComplexFilter,
                        structureFilter,
                        structureChecker,
                        _categories[filterProvider.Key],
                        placerInitializer,
                        mepElementsToFilter));
                }
            };
            return placers;
        }

        private List<OpeningPlacer> GetFittingPlacers(
            Filter structureFilter,
            Func<MepCategory[], IClashChecker> structureCheckerFunc,
            IFittingPlacerInitializer placerInitializer,
            ElementId[] mepElementsToFilter) {

            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            foreach(var filterProvider in _fittingFilterProviders) {
                MepCategory[] mepCategories = _categories.GetCategories(filterProvider.Key)
                    .Where(category => category.IsSelected)
                    .ToArray();
                if(mepCategories.Any(category => IntersectionWithStructureEnabled(category, structureFilter.Name))) {
                    //получение стандартного фильтра по категории
                    Filter mepStandardFilter = GetFittingFilter(filterProvider.Value);
                    //добавление к стандартному фильтру критериев по параметрам, созданных пользователем
                    Filter mepComplexFilter = CreateMepCategoriesAndFilterSet(
                        _revitRepository.GetClashRevitRepository(),
                        mepStandardFilter,
                        mepCategories);
                    placers.AddRange(GetFittingPlacers(
                        mepComplexFilter,
                        structureFilter,
                        structureCheckerFunc.Invoke(mepCategories),
                        placerInitializer,
                        mepElementsToFilter,
                        mepCategories));
                }
            };
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
            if(minSize != null) {
                return filterProvider.Invoke(_revitRepository.GetClashRevitRepository(), minSize.Value);
            }
            return null;
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
        /// <returns></returns>
        private Filter CreateMepCategoriesAndFilterSet(RevitClashDetective.Models.RevitRepository revitRepository, Filter mepFilterToAdd, params MepCategory[] mepCategories) {
            var mepCategoriesAndFilterSet = new Set() {
                SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.Or),
                RevitRepository = revitRepository,
                Criteria = new List<Criterion>()
            };
            foreach(MepCategory category in mepCategories) {
                var mepCategoryAndFilterSet = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    RevitRepository = revitRepository,
                    Criteria = new List<Criterion>() {
                        mepFilterToAdd.Set,
                        GetSetFromMepCategory(revitRepository, category)
                    }
                };
                mepCategoriesAndFilterSet.Criteria.Add(mepCategoryAndFilterSet);
            }
            return new Filter(revitRepository) {
                CategoryIds = new List<ElementId>(mepFilterToAdd.CategoryIds),
                Name = mepFilterToAdd.Name,
                Set = mepCategoriesAndFilterSet
            };
        }

        /// <summary>
        /// Возвращает поисковый набор из заданной конфигурации настроек инженерной системы
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="mepCategory">Конфигурация настроек инженерной системы</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private Set GetSetFromMepCategory(RevitClashDetective.Models.RevitRepository revitRepository, MepCategory mepCategory) {
            if(mepCategory is null) {
                throw new ArgumentNullException(nameof(mepCategory));
            }
            if(mepCategory.Set is null) {
                throw new ArgumentNullException(nameof(mepCategory.Set));
            }
            var set = new Set() {
                SetEvaluator = mepCategory.Set.SetEvaluator,
                RevitRepository = revitRepository,
                Criteria = new List<Criterion>()
            };
            foreach(Criterion criterion in mepCategory.Set.Criteria) {
                criterion.SetRevitRepository(revitRepository);
                set.Criteria.Add(criterion);
            }
            return set;
        }
    }
}
