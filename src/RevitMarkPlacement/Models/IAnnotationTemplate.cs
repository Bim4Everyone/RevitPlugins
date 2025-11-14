using Autodesk.Revit.DB;

using RevitMarkPlacement.Models.AnnotationTemplates;

namespace RevitMarkPlacement.Models;

internal interface IAnnotationTemplate {
    SpotDimension SpotDimension { get; }
    AnnotationSymbolType AnnotationSymbolType { get; }
    
    AnnotationInstance Process(AnnotationTemplateOptions annotationTemplateOptions);
}
