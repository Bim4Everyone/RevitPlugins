using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions;
internal static class ConnectorExtension {
    public static double GetArea(this Connector connector) {
        return connector.Shape switch {
            ConnectorProfileType.Round => Math.PI * Math.Pow(connector.Radius, 2),
            ConnectorProfileType.Rectangular => connector.Width * connector.Height,
            ConnectorProfileType.Oval => Math.PI * connector.Height * connector.Width / 4,
            _ => 0,
        };
    }

    public static double GetDiameter(this Connector connector) {
        return connector.Shape switch {
            ConnectorProfileType.Round => connector.Radius * 2,
            _ => 0,
        };
    }

    public static double GetHeight(this Connector connector) {
        return connector.Shape switch {
            ConnectorProfileType.Rectangular => connector.Height,
            _ => 0,
        };
    }

    public static double GetWidth(this Connector connector) {
        return connector.Shape switch {
            ConnectorProfileType.Rectangular => connector.Width,
            _ => 0,
        };
    }
}
