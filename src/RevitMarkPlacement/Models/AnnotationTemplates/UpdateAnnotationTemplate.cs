using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models.AnnotationTemplates;

internal sealed class UpdateAnnotationTemplateOptions : AnnotationTemplateOptions {
}

internal sealed class UpdateAnnotationTemplate : AnnotationTemplate<UpdateAnnotationTemplateOptions> {
    private AnnotationSymbol _annotationSymbol;

    public UpdateAnnotationTemplate(
        SpotDimension spotDimension,
        AnnotationSymbol annotationSymbol,
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig)
        : base(spotDimension, annotationSymbol.AnnotationSymbolType, revitRepository, systemPluginConfig) {
        _annotationSymbol = annotationSymbol;
    }

    protected override AnnotationSymbol ProcessAnnotation(UpdateAnnotationTemplateOptions annotationTemplateOptions) {
        int levelCount = _annotationSymbol.GetParamValueOrDefault<int>(_systemPluginConfig.LevelCountParamName);
        double levelHeightMeter = _annotationSymbol.GetParamValueOrDefault<double>(_systemPluginConfig.LevelHeightParamName);

        _revitRepository.DeleteElement(_annotationSymbol);
        return _annotationSymbol = CreateAnnotation(levelCount, levelHeightMeter * 1000);
    }
}
