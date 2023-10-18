using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitRoomTagPlacement.Models {
    internal class PathTriangle {
        public XYZ Vertex1;
        public XYZ Vertex2;
        public XYZ Vertex3;

        public List<XYZ> Vertices;

        public XYZ Center;
        public double Weight;

        public bool IsViisted;

        public List<PathTriangle> NextTriangles;

        public PathTriangle(MeshTriangle triangle) {
            Vertex1 = triangle.get_Vertex(0);
            Vertex2 = triangle.get_Vertex(1);
            Vertex3 = triangle.get_Vertex(2);

            Vertices = new List<XYZ>() { Vertex1, Vertex2, Vertex3};

            Center = GetCenter();
            Weight = GetWeight();

            IsViisted = false;

            NextTriangles = new List<PathTriangle>();
        }

        private double GetWeight() {
            double value1 = (Vertex2.X - Vertex1.X) * (Vertex3.Y - Vertex1.Y);
            double value2 = (Vertex3.X - Vertex1.X) * (Vertex2.Y - Vertex1.Y);
            return Math.Abs(value1 - value2) / 2;
        }

        private XYZ GetCenter() {
            double CenterX = (Vertex1.X + Vertex2.X + Vertex3.X) / 3;
            double CenterY = (Vertex1.Y + Vertex2.Y + Vertex3.Y) / 3;
            double CenterZ = (Vertex1.Z + Vertex2.Z + Vertex3.Z) / 3;
            return new XYZ(CenterX, CenterY, CenterZ);
        }
    }
}
