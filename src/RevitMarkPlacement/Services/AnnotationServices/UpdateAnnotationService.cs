using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitMarkPlacement.Comparators;
using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.AnnotationTemplates;

namespace RevitMarkPlacement.Services.AnnotationServices;

internal sealed class UpdateAnnotationService : AnnotationService {
    private readonly IEqualityComparer<SpotDimension> _spotDimensionComparer = new ElementIdComparer<SpotDimension>();

    public UpdateAnnotationService(RevitRepository revitRepository, SystemPluginConfig systemPluginConfig)
        : base(revitRepository, systemPluginConfig) {
    }

    protected override IEnumerable<IAnnotationTemplate> LoadAnnotationsImpl(ICollection<SpotDimension> spotDimensions) {
        if(spotDimensions.Count == 0) {
            return _revitRepository.GetAnnotationSymbols()
                .Select(item => new UpdateAnnotationTemplate(
                    GetSpotDimension(item),
                    item,
                    _revitRepository,
                    _systemPluginConfig))
                .ToArray();
        }

        return SelectedAnnotationTemplates(spotDimensions);
    }

    private IEnumerable<IAnnotationTemplate> SelectedAnnotationTemplates(ICollection<SpotDimension> spotDimensions) {
        foreach(var annotationSymbol in _revitRepository.GetAnnotationSymbols()) {
            SpotDimension spotDimension = GetSpotDimension(annotationSymbol);
            if(spotDimensions.Contains(spotDimension, _spotDimensionComparer)) {
                yield return new UpdateAnnotationTemplate(
                    spotDimension,
                    annotationSymbol,
                    _revitRepository,
                    _systemPluginConfig);
            }
        }
    }

    private SpotDimension GetSpotDimension(AnnotationSymbol annotationSymbol) {
        return _revitRepository.GetElement<SpotDimension>(GetSpotDimensionId(annotationSymbol));
    }
}
