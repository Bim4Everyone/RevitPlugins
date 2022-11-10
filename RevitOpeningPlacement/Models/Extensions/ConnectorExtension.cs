using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class ConnectorExtension {
        public static double GetArea(this Connector connector) {
            switch(connector.Shape) {
                case ConnectorProfileType.Round:
                return Math.PI * Math.Pow(connector.Radius, 2);
                case ConnectorProfileType.Rectangular:
                return connector.Width * connector.Height;
                case ConnectorProfileType.Oval:
                return Math.PI * connector.Height * connector.Width / 4;
                default:
                return 0;
            }
        }
    }
}
