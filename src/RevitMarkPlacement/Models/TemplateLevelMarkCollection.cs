using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models {
    internal class TemplateLevelMarkCollection {
        private readonly RevitRepository _revitRepository;
        private ISelectionMode _selectionMode;

        public TemplateLevelMarkCollection(RevitRepository revitRepository, ISelectionMode selectionMode) {
            _revitRepository = revitRepository;
            _selectionMode = selectionMode;
            InitializeTemplateLevelMarks();
        }

        public List<TemplateLevelMark> TemplateLevelMarks { get; set; }

        public void PlaceAnnotation() {
        }

        public void CreateAnnotation(int floorCount, double floorHeight) {
            using(TransactionGroup t = _revitRepository.StartTransactionGroup("Обновление и расстановка аннотаций")) {
                foreach(var mark in TemplateLevelMarks) {
                    if(mark.Annotation == null) {
                        mark.CreateAnnotation(floorCount, floorHeight);
                    } else {
                        mark.OverwriteAnnotation(floorCount, floorHeight);
                    }
                }

                t.Assimilate();
            }
        }

        public void UpdateAnnotation() {
            using(TransactionGroup t = _revitRepository.StartTransactionGroup("Обновление и расстановка аннотаций")) {
                foreach(var mark in TemplateLevelMarks) {
                    if(mark.Annotation != null) {
                        mark.UpdateAnnotation();
                    }
                }

                t.Assimilate();
            }
        }

        private void InitializeTemplateLevelMarks() {
            var spots = _revitRepository.GetSpotDimensions(_selectionMode).ToList();
            var annotations = _revitRepository.GetAnnotations().ToList();
            TemplateLevelMarks = new List<TemplateLevelMark>();
            var symbols = _revitRepository.GetAnnotationSymbols();
            foreach(var annotation in annotations) {
                // могут быть проблемы, если идентификатор будет больше int
                var spotId = annotation.GetParamValueOrDefault<int>(RevitRepository.SpotDimensionIdParam);
#if REVIT_2023_OR_LESS
                var spot = _revitRepository.GetElement(new ElementId(spotId)) as SpotDimension;
#else
                var spot = _revitRepository.GetElement(new ElementId((long) spotId)) as SpotDimension;
#endif

                if(spot == null) {
                    _revitRepository.DeleteElement(annotation);
                }

                if(spots.Contains(spot, new ElementNameEquatable<SpotDimension>())) {
                    var position = _revitRepository.GetSpotOrientation(spot, symbols);
                    var annotationManager = new AnnotationManager(_revitRepository, position);
                    TemplateLevelMarks.Add(new TemplateLevelMark(spot, annotationManager, annotation));
                }
            }

            TemplateLevelMarks.AddRange(spots
                .Except(TemplateLevelMarks.Select(m => m.SpotDimension), new ElementNameEquatable<SpotDimension>())
                .Select(s => new TemplateLevelMark(s,
                    new AnnotationManager(_revitRepository, _revitRepository.GetSpotOrientation(s, symbols)))));
        }
    }
}