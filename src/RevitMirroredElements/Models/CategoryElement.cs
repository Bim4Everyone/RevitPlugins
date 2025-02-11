using Autodesk.Revit.DB;

namespace RevitMirroredElements.Models {
    internal class CategoryElement {
        public string Name { get; set; }
        public ElementId Id { get; set; }
        public Category Category { get; set; }
        public bool IsSelected { get; set; }
    }
}
