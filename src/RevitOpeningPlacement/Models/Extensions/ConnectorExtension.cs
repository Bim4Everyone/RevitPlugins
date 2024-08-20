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

        public static double GetDiameter(this Connector connector) {
            switch(connector.Shape) {
                case ConnectorProfileType.Round:
                return connector.Radius * 2;
                default:
                return 0;
            }
        }

        public static double GetHeight(this Connector connector) {
            switch(connector.Shape) {
                case ConnectorProfileType.Rectangular:
                return connector.Height;
                default:
                return 0;
            }
        }

        public static double GetWidth(this Connector connector) {
            switch(connector.Shape) {
                case ConnectorProfileType.Rectangular:
                return connector.Width;
                default:
                return 0;
            }
        }
    }
}
