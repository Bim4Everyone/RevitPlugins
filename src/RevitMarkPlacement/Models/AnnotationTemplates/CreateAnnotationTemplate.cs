using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models.AnnotationTemplates;

internal class CreateAnnotationTemplateOptions : AnnotationTemplateOptions {
    public int LevelCount { get; set; }
    public double LevelHeightMm { get; set; }
}

internal sealed class CreateAnnotationTemplate : AnnotationTemplate<CreateAnnotationTemplateOptions> {
    public CreateAnnotationTemplate(
        SpotDimension spotDimension,
        AnnotationSymbolType annotationSymbolType,
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig)
        : base(spotDimension, annotationSymbolType, revitRepository, systemPluginConfig) {
    }

    protected override AnnotationSymbol ProcessAnnotation(CreateAnnotationTemplateOptions annotationTemplateOptions) {
        return CreateAnnotation(annotationTemplateOptions.LevelCount, annotationTemplateOptions.LevelHeightMm);
    }
}
