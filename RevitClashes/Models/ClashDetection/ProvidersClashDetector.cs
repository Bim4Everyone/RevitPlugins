using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
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

        public ProvidersClashDetector(RevitRepository revitRepository, IProvider firstProvider, IProvider secondProvider) {
            _revitRepository = revitRepository;
            _firstProvider = firstProvider;
            _secondProvider = secondProvider;

            _mainElements = _firstProvider.GetElements();
            _ids = _mainElements.Select(item => item.Id).ToList();

            _secondElements = _secondProvider.GetElements();
        }

        public List<ClashModel> GetClashes() {
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = 100;
                pb.DisplayTitleFormat = "Идёт расчёт... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = _secondElements.Count;
                var ct = pb.CreateCancellationToken();

                pb.Show();

                return GetClashes(progress, ct).Distinct().ToList();
            }
        }

        public List<ClashModel> GetClashes(IProgress<int> progress, CancellationToken ct) {
            List<ClashModel> clashes = new List<ClashModel>();

            if(_mainElements.Count == 0) {
                return clashes;
            }

            var resultTransform = _firstProvider.MainTransform.GetTransitionMatrix(_secondProvider.MainTransform);

            var count = 0;
            foreach(var element in _secondElements) {
                progress.Report(count++);
                ct.ThrowIfCancellationRequested();

                var solids = _secondProvider.GetSolids(element)
                                            .ToArray();

                // Solid-ы необходимо проверять на замкнутость геометрии до трансформации, так как у трансформированного solid-а 
                // появляютя грани и ребра
                var closedSolids = solids.Where(item => !item.IsNotClosed())
                                         .Select(item => SolidUtils.CreateTransformed(item, resultTransform));

                var openSolids = solids.Where(item => item.IsNotClosed())
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
                                .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                                .WherePasses(new ElementIntersectsSolidFilter(solid))
                                .Where(item => item.Id != element.Id)
                                .Select(item => new ClashModel(_revitRepository, item, element));
        }

        private IEnumerable<ClashModel> GetElementClashesWithIntersection(Element element, Solid solid) {
            return new FilteredElementCollector(_firstProvider.Doc, _ids)
                                .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                                .Where(item => item.Id != element.Id)
                                .Where(item => HasSolidsIntersection(item, solid))
                                .Select(item => new ClashModel(_revitRepository, item, element));
        }

        private bool HasSolidsIntersection(Element element, Solid solid) {
            foreach(var s in _firstProvider.GetSolids(element)) {
                if(s.IsNotClosed()) {
                    return new ElementIntersectsSolidFilter(solid).PassesFilter(element);
                }
                if(HasSolidIntersection(s, solid) || HasSolidIntersection(solid, s)) {
                    return true;
                }
            }

            return false;
        }

        private bool HasSolidIntersection(Solid solid0, Solid solid1) {
            try {
                var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solid0, solid1, BooleanOperationsType.Intersect);
                return intersection.Volume > 0;
            } catch {
                return false;
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
