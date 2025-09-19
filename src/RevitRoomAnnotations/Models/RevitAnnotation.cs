using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitRoomAnnotations.Models;
public class RevitAnnotation {
    private readonly Element _annotation;
    public RevitAnnotation(Element annotation) {
        _annotation = annotation ?? throw new ArgumentNullException(nameof(annotation));
    }

    public Element Annotation => _annotation;
    public ElementId Id => _annotation.Id;

    public int RoomIdInAnnotation => _annotation.GetParamValueOrDefault<int>("ФОП_ID_Комнаты");
    public int LinkInstIdInAnnotation => _annotation.GetParamValueOrDefault<int>("ФОП_ID_Связи");
}

