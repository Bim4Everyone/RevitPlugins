using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection {
    internal class ProvidersClashDetector {
        private readonly RevitRepository _revitRepository;
        private readonly IProvider _firstProvider;
        private readonly IProvider _secondProvider;
        private readonly List<ElementId> _ids;
        private readonly List<Element> _mainElements;
        private readonly List<Element> _secondElements;
        private readonly int _progressBarStep = 100;


        public ProvidersClashDetector(
            RevitRepository revitRepository,
            IProvider firstProvider,
            IProvider secondProvider) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _firstProvider = firstProvider ?? throw new ArgumentNullException(nameof(firstProvider));
            _secondProvider = secondProvider ?? throw new ArgumentNullException(nameof(secondProvider));

            _mainElements = _firstProvider.GetElements();
            _ids = _mainElements.Select(item => item.Id).ToList();

            _secondElements = _secondProvider.GetElements();
        }

        public List<ClashModel> GetClashes() {
            if(_mainElements.Count == 0) {
                return new List<ClashModel>();
            }
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = _progressBarStep;
                pb.DisplayTitleFormat = "Идёт расчёт... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = _secondElements.Count;
                var ct = pb.CreateCancellationToken();

                pb.Show();

                return GetClashes(progress, ct).Distinct().ToList();
            }
        }

        private List<ClashModel> GetClashes(IProgress<int> progress, CancellationToken ct) {
            List<ClashModel> clashes = new List<ClashModel>();

            if(_mainElements.Count == 0) {
                return clashes;
            }

            var resultTransform = _firstProvider.MainTransform.GetTransitionMatrix(_secondProvider.MainTransform);

            var count = 0;
            foreach(var element in _secondElements) {
                ct.ThrowIfCancellationRequested();
                progress.Report(count);
                count++;

                var solids = _secondProvider.GetSolids(element)
                                            .ToArray();

                var notNullSolids = solids.Where(s => s != null);
                // Solid-ы необходимо проверять на замкнутость геометрии до трансформации, так как у трансформированного solid-а 
                // появляются грани и ребра
                var closedSolids = notNullSolids.Where(item => !item.IsNotClosed())
                                         .Select(item => SolidUtils.CreateTransformed(item, resultTransform));

                var openSolids = notNullSolids.Where(item => item.IsNotClosed())
                                       .Select(item => SolidUtils.CreateTransformed(item, resultTransform));

                foreach(var solid in openSolids) {
                    clashes.AddRange(GetElementClashesWithFilter(element, solid));
                }

                foreach(var solid in closedSolids) {
                    clashes.AddRange(GetElementClashesWithIntersection(element, solid));
                }
            }

            return clashes;
        }

        private IEnumerable<ClashModel> GetElementClashesWithFilter(Element element, Solid solid) {
            return new FilteredElementCollector(_firstProvider.Doc, _ids)
                .Excluding(new ElementId[] { element.Id })
                .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(solid))
                .Select(item => new ClashModel(
                    _revitRepository,
                    item,
                    _firstProvider.MainTransform,
                    element,
                    _secondProvider.MainTransform));
        }

        private IEnumerable<ClashModel> GetElementClashesWithIntersection(Element element, Solid solid) {
            return new FilteredElementCollector(_firstProvider.Doc, _ids)
                    .Excluding(new ElementId[] { element.Id })
                    .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                    .Where(item => HasSolidsIntersection(item, solid))
                    .Select(item => new ClashModel(
                        _revitRepository,
                        item,
                        _firstProvider.MainTransform,
                        element,
                        _secondProvider.MainTransform));
        }

        private bool HasSolidsIntersection(Element element, Solid solid) {
            foreach(var s in _firstProvider.GetSolids(element)) {
                if(s != null && s.IsNotClosed()) {
                    return new ElementIntersectsSolidFilter(solid).PassesFilter(element);
                }
                if(HasSolidIntersection(element, s, solid)) {
                    return true;
                }
            }

            return false;
        }

        private bool HasSolidIntersection(Element element, Solid solid0, Solid solid1) {
            try {
                var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    solid0,
                    solid1,
                    BooleanOperationsType.Intersect);
                return intersection.Volume > 0;
            } catch {
                return new ElementIntersectsSolidFilter(solid1).PassesFilter(element);
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
