using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class PlacementConfigurator {
        private readonly RevitRepository _revitRepository;
        private readonly MepCategoryCollection _categories;
        private List<UnplacedClashModel> _unplacedClashes = new List<UnplacedClashModel>();

        private Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter>> _rectangleMepFilterProviders =
            new Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter>> {
                { MepCategoryEnum.RectangleDuct, (revitRepository, height, width) => { return FiltersInitializer.GetRectangleDuctFilter(revitRepository, height, width); } },
                { MepCategoryEnum.CableTray, (revitRepository, height, width) => { return FiltersInitializer.GetTrayFilter(revitRepository, height, width); } },
            };

        private Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, Filter>> _roundMepFilterProviders =
            new Dictionary<MepCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, double, Filter>> {
                { MepCategoryEnum.Pipe, (revitRepository, diameter) => { return FiltersInitializer.GetPipeFilter(revitRepository, diameter); } },
                { MepCategoryEnum.RoundDuct, (revitRepository, diameter) => { return FiltersInitializer.GetRoundDuctFilter(revitRepository, diameter); } },
                { MepCategoryEnum.Conduit, (revitRepository, diameter) => { return FiltersInitializer.GetConduitFilter(revitRepository, diameter); } },
            };

        private Dictionary<FittingCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, Filter>> _fittingFilterProviders =
            new Dictionary<FittingCategoryEnum, Func<RevitClashDetective.Models.RevitRepository, Filter>> {
                { FittingCategoryEnum.PipeFitting, (revitRepository) => { return FiltersInitializer.GetPipeFittingFilter(revitRepository); } },
                { FittingCategoryEnum.CableTrayFitting, (revitRepository) => { return FiltersInitializer.GetTrayFittingFilter(revitRepository); } },
                { FittingCategoryEnum.ConduitFitting, (revitRepository) => { return FiltersInitializer.GetConduitFittingFilter(revitRepository); } },
                { FittingCategoryEnum.DuctFitting, (revitRepository) => { return FiltersInitializer.GetDuctFittingFilter(revitRepository); } },
            };

        public PlacementConfigurator(RevitRepository revitRepository, MepCategoryCollection categories) {
            _revitRepository = revitRepository;
            _categories = categories;
        }

        public IEnumerable<OpeningPlacer> GetPlacers() {
            var wallFilter = FiltersInitializer.GetWallFilter(_revitRepository.GetClashRevitRepository());
            var floorFilter = FiltersInitializer.GetFloorFilter(_revitRepository.GetClashRevitRepository());

            var mepCurveWallClashChecker = ClashChecker.GetMepCurveWallClashChecker(_revitRepository);
            var mepCurveFloorClashChecker = ClashChecker.GetMepCurveFloorClashChecker(_revitRepository);

            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            placers.AddRange(GetRoundMepPlacers(wallFilter, mepCurveWallClashChecker, new RoundMepWallPlacerInitializer()));
            placers.AddRange(GetRoundMepPlacers(floorFilter, mepCurveFloorClashChecker, new RoundMepFloorPlacerInitializer()));
            placers.AddRange(GetRectangleMepPlacers(wallFilter, mepCurveWallClashChecker, new RectangleMepWallPlacerInitializer()));
            placers.AddRange(GetRectangleMepPlacers(floorFilter, mepCurveFloorClashChecker, new RectangleMepFloorPlacerInitializer()));
            placers.AddRange(GetFittingPlacers(floorFilter,
                (categoties) => ClashChecker.GetFittingFloorClashChecker(_revitRepository, categoties),
                new FittingFloorPlacerInitializer()));
            placers.AddRange(GetFittingPlacers(wallFilter,
                (categoties) => ClashChecker.GetFittingWallClashChecker(_revitRepository, categoties),
                new FittingWallPlacerInitializer()));
            return placers;
        }

        public List<UnplacedClashModel> GetUnplacedClashes() {
            return _unplacedClashes;
        }

        private List<OpeningPlacer> GetRoundMepPlacers(Filter structureFilter, IClashChecker structureChecker, IMepCurvePlacerInitializer placerInitializer) {
            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            foreach(var filterProvider in _roundMepFilterProviders) {
                var mepFilter = GetRoundMepFilter(filterProvider.Key, filterProvider.Value);
                placers.AddRange(GetMepPlacers(mepFilter, structureFilter, structureChecker, _categories[filterProvider.Key], placerInitializer));
            };
            return placers;
        }

        private List<OpeningPlacer> GetRectangleMepPlacers(Filter structureFilter, IClashChecker structureChecker, IMepCurvePlacerInitializer placerInitializer) {
            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            foreach(var filterProvider in _rectangleMepFilterProviders) {
                var mepFilter = GetRectangleMepFilter(filterProvider.Key, filterProvider.Value);
                placers.AddRange(GetMepPlacers(mepFilter, structureFilter, structureChecker, _categories[filterProvider.Key], placerInitializer));
            };
            return placers;
        }

        private List<OpeningPlacer> GetFittingPlacers(Filter structureFilter, Func<MepCategory[], IClashChecker> structureCheckerFunc, IFittingPlacerInitializer placerInitializer) {
            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            foreach(var filterProvider in _fittingFilterProviders) {
                var mepFilter = GetFittingFilter(filterProvider.Key, filterProvider.Value);
                var categoties = _categories.GetCategoties(filterProvider.Key).ToArray();
                placers.AddRange(GetFittingPlacers(mepFilter,
                    structureFilter,
                    structureCheckerFunc.Invoke(categoties),
                    placerInitializer,
                    categoties));
            };
            return placers;
        }

        private IEnumerable<OpeningPlacer> GetMepPlacers(Filter mepFilter, Filter structureFilter, IClashChecker clashChecker, MepCategory mepCategory, IMepCurvePlacerInitializer placerInitializer) {
            return GetClashes(mepFilter, structureFilter, clashChecker).Select(item => placerInitializer.GetPlacer(_revitRepository, item, mepCategory));
        }

        private IEnumerable<OpeningPlacer> GetFittingPlacers(Filter mepFilter, Filter structureFilter, IClashChecker clashChecker, IFittingPlacerInitializer placerInitializer, params MepCategory[] mepCategories) {
            return GetClashes(mepFilter, structureFilter, clashChecker).Select(item => placerInitializer.GetPlacer(_revitRepository, item, mepCategories));
        }

        private IEnumerable<ClashModel> GetClashes(Filter mepFilter, Filter constructionFilter, IClashChecker clashChecker) {
            var wallClashes = ClashInitializer.GetClashes(_revitRepository.GetClashRevitRepository(), mepFilter, constructionFilter)
                .ToList();

            _unplacedClashes.AddRange(wallClashes
                .Select(item => new UnplacedClashModel() {
                    Message = clashChecker.Check(item),
                    Clash = item
                })
                .Where(item => !string.IsNullOrEmpty(item.Message) && !item.Message.Equals(RevitRepository.SystemCheck, StringComparison.CurrentCulture)));
            return wallClashes.Where(item => string.IsNullOrEmpty(clashChecker.Check(item)));
        }

        private Filter GetRoundMepFilter(MepCategoryEnum category, Func<RevitClashDetective.Models.RevitRepository, double, Filter> filterProvider) {
            var minSize = _categories[category]?.MinSizes[Parameters.Diameter];
            if(minSize != null) {
                return filterProvider.Invoke(_revitRepository.GetClashRevitRepository(), minSize.Value);
            }
            return null;
        }

        private Filter GetRectangleMepFilter(MepCategoryEnum category, Func<RevitClashDetective.Models.RevitRepository, double, double, Filter> filterProvider) {
            var minSizes = _categories[MepCategoryEnum.CableTray]?.MinSizes;
            if(minSizes != null) {
                var height = minSizes[Parameters.Height];
                var width = minSizes[Parameters.Width];
                if(height != null && width != null) {
                    return filterProvider.Invoke(_revitRepository.GetClashRevitRepository(), height.Value, width.Value);
                }
            }
            return null;
        }

        private Filter GetFittingFilter(FittingCategoryEnum category, Func<RevitClashDetective.Models.RevitRepository, Filter> filterProvider) {
            return filterProvider.Invoke(_revitRepository.GetClashRevitRepository());
        }
    }
}
