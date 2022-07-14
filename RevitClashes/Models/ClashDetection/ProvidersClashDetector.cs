using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                return GetClashes(progress, ct);
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
                    .Select(item => SolidUtils.CreateTransformed(item, resultTransform));

                foreach(var solid in solids) {
                    clashes.AddRange(GetElementClashes(element, solid));
                }
            }

            return clashes;
        }

        private IEnumerable<ClashModel> GetElementClashes(Element element, Solid solid) {
            return new FilteredElementCollector(_firstProvider.Doc, _ids)
                                .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                                .WherePasses(new ElementIntersectsSolidFilter(solid))
                                .Where(item => item.Id != element.Id)
                                .Select(item => new ClashModel(_revitRepository, item, element));
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
