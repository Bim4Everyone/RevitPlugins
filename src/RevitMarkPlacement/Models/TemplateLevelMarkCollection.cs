using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models;

internal class TemplateLevelMarkCollection {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ISpotDimensionSelection _selection;

    public TemplateLevelMarkCollection(
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig,
        ISpotDimensionSelection selection) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _selection = selection;
        InitializeTemplateLevelMarks();
    }

    public List<TemplateLevelMark> TemplateLevelMarks { get; set; }

    public void PlaceAnnotation() {
    }

    public void CreateAnnotation(int floorCount, double floorHeight) {
        using(var t = _revitRepository.StartTransactionGroup("Обновление и расстановка аннотаций")) {
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
        using(var t = _revitRepository.StartTransactionGroup("Обновление и расстановка аннотаций")) {
            foreach(var mark in TemplateLevelMarks) {
                if(mark.Annotation != null) {
                    mark.UpdateAnnotation();
                }
            }

            t.Assimilate();
        }
    }

    private void InitializeTemplateLevelMarks() {
        var spots = _revitRepository.GetSpotDimensions(_selection).ToList();
        var annotations = _revitRepository.GetAnnotations().ToList();
        TemplateLevelMarks = new List<TemplateLevelMark>();
        var symbols = _revitRepository.GetAnnotationSymbols();
        foreach(var annotation in annotations) {
            // могут быть проблемы, если идентификатор будет больше int
            int spotId = annotation.GetParamValueOrDefault<int>(_systemPluginConfig.SpotDimensionIdParamName);
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
                var annotationManager = new AnnotationManager(_revitRepository, _systemPluginConfig, position);
                TemplateLevelMarks.Add(new TemplateLevelMark(spot, annotationManager, annotation));
            }
        }

        TemplateLevelMarks.AddRange(
            spots
                .Except(TemplateLevelMarks.Select(m => m.SpotDimension), new ElementNameEquatable<SpotDimension>())
                .Select(s => new TemplateLevelMark(
                    s,
                    new AnnotationManager(
                        _revitRepository,
                        _systemPluginConfig,
                        _revitRepository.GetSpotOrientation(s, symbols)))));
    }
}
