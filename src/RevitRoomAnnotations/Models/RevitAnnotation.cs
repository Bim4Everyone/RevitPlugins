using System;

using Autodesk.Revit.DB;


namespace RevitRoomAnnotations.Models;
public class RevitAnnotation {
    public RevitAnnotation(Element annotation) {
        Annotation = annotation ?? throw new ArgumentNullException(nameof(annotation));
    }

    public Element Annotation { get; }
    public string CombinedID { get; set; }
}

