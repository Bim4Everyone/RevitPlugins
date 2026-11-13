using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Entities {
    internal class ElementReplacement {
        public ElementReplacement(ElementId oldElementId, Element newElement) {
            OldElementId = oldElementId;
            NewElement = newElement;
        }

        public ElementId OldElementId { get; }
        public Element NewElement { get; }
    }
}
