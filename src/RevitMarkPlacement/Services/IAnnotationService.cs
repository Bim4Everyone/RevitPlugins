using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitMarkPlacement.Models.AnnotationTemplates;

namespace RevitMarkPlacement.Services;

internal interface IAnnotationService {
    void LoadAnnotations(ICollection<SpotDimension> spotDimensions);
    void ProcessAnnotations(AnnotationTemplateOptions annotationTemplateOptions);
}
