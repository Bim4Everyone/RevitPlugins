using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitRoomTagPlacement.Models {
    internal class PathTriangle {
        private readonly XYZ _vertex1;
        private readonly XYZ _vertex2;
        private readonly XYZ _vertex3;
        private readonly XYZ _center;

        private readonly List<XYZ> _vertices;

        private readonly double _weight;

        public PathTriangle(MeshTriangle triangle) {
            _vertex1 = triangle.get_Vertex(0);
            _vertex2 = triangle.get_Vertex(1);
            _vertex3 = triangle.get_Vertex(2);

            _vertices = new List<XYZ>() { Vertex1, Vertex2, Vertex3};

            _center = GetCenter();
            _weight = GetWeight();

            IsVisited = false;
            NextTriangles = new List<PathTriangle>();
        }

        public XYZ Vertex1 => _vertex1;
        public XYZ Vertex2 => _vertex2;
        public XYZ Vertex3 => _vertex3;
        public XYZ Center => _center;
        public List<XYZ> Vertices => _vertices;
        public double Weight => _weight;

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
}
