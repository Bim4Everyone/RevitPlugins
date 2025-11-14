using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.AnnotationTemplates;

namespace RevitMarkPlacement.Services.AnnotationServices;

internal abstract class AnnotationService : IAnnotationService {
    protected readonly RevitRepository _revitRepository;
    protected readonly SystemPluginConfig _systemPluginConfig;

    protected AnnotationService(
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
    }

    public IReadOnlyCollection<IAnnotationTemplate> AnnotationTemplate { get; private set; }

    public void LoadAnnotations(ICollection<SpotDimension> spotDimensions) {
        AnnotationTemplate = LoadAnnotationsImpl(spotDimensions).ToArray();
    }

    public void ProcessAnnotations(AnnotationTemplateOptions annotationTemplateOptions) {
        foreach(var annotationTemplate in AnnotationTemplate) {
            annotationTemplate.Process(annotationTemplateOptions);
        }
    }

    protected abstract IEnumerable<IAnnotationTemplate> LoadAnnotationsImpl(ICollection<SpotDimension> spotDimensions);

    /// <summary>
    /// Возвращает высотные отметки.
    /// </summary>
    /// <param name="annotation">Аннотация.</param>
    /// <returns>Возвращает высотную отметку по идентификатору из параметра.</returns>
    /// <remarks>HACK: могут быть проблемы, если идентификатор будет больше int</remarks>
    protected ElementId GetSpotDimensionId(AnnotationSymbol annotation) {
        int? spotId = annotation.GetParamValueOrDefault<int?>(_systemPluginConfig.SpotDimensionIdParamName);
        if(spotId is null) {
            return ElementId.InvalidElementId;
        }

#if REVIT_2023_OR_LESS
        return new ElementId((int) spotId);
#else
        return new ElementId((long) spotId);
#endif
    }
}
