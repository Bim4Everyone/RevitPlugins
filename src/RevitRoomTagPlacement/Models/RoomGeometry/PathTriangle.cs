using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitRoomTagPlacement.Models;
internal class PathTriangle {
    public PathTriangle(MeshTriangle triangle) {
        Vertex1 = triangle.get_Vertex(0);
        Vertex2 = triangle.get_Vertex(1);
        Vertex3 = triangle.get_Vertex(2);

        Vertices = [Vertex1, Vertex2, Vertex3];

        Center = GetCenter();
        Weight = GetWeight();

        IsVisited = false;
        NextTriangles = [];
    }

    public XYZ Vertex1 { get; }
    public XYZ Vertex2 { get; }
    public XYZ Vertex3 { get; }
    public XYZ Center { get; }
    public List<XYZ> Vertices { get; }
    public double Weight { get; }

    public bool IsVisited { get; set; }
    public List<PathTriangle> NextTriangles { get; set; }

    private double GetWeight() {
        double value1 = (Vertex2.X - Vertex1.X) * (Vertex3.Y - Vertex1.Y);
        double value2 = (Vertex3.X - Vertex1.X) * (Vertex2.Y - Vertex1.Y);
        return Math.Abs(value1 - value2) / 2;
    }

    private XYZ GetCenter() {
        double centerX = (Vertex1.X + Vertex2.X + Vertex3.X) / 3;
        double centerY = (Vertex1.Y + Vertex2.Y + Vertex3.Y) / 3;
        double centerZ = (Vertex1.Z + Vertex2.Z + Vertex3.Z) / 3;
        return new XYZ(centerX, centerY, centerZ);
    }
}
