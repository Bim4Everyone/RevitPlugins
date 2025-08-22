using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitRoomAnnotations.Models;
public class RevitAnnotation {
    public RevitAnnotation(Element annotation) {
        Annotation = annotation ?? throw new ArgumentNullException(nameof(annotation));

        Id = annotation.Id;

        LinkId = annotation.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.FopId.Name);

        FileName = annotation.Name ?? string.Empty;
    }

    public Element Annotation { get; }
    public ElementId Id { get; }
    public string LinkId { get; }
    public string FileName { get; }
}

