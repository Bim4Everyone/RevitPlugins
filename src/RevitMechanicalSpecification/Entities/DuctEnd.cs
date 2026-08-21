using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Entities {
    internal class DuctEnd {
        public Connector DuctConnector { get; set; }
        public XYZ Origin { get; set; }
        public List<ConnectorReference> Connections { get; } = new List<ConnectorReference>();
    }
}
