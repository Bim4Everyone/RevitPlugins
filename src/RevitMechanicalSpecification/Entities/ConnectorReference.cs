using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Entities {
    internal class ConnectorReference {
        public Connector Connector { get; set; }
        public int ConnectorId { get; set; }
        public Element Owner { get; set; }
        public ElementId OwnerId { get; set; }
        public XYZ Origin { get; set; }
    }
}
