using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

namespace RevitRoomTagPlacement.Models {
    internal class TriangulatedRoom {
        private readonly Room _room;
        private readonly Mesh _mesh;
        private readonly BoundingBoxUV _roomBB;
        private readonly List<PathTriangle> _triangles;
        private readonly PathTriangle _minTriangle;

        public TriangulatedRoom(Room room) {
            _room = room;
            PlanarFace horizontalFace = GetRoomHorizontalFace();
            _mesh = horizontalFace.Triangulate();
            _roomBB = horizontalFace.GetBoundingBox();
            _triangles = GetTriangleList();
            _minTriangle = GetMinTriangle();
            FindNextTriangles();
        }

        public List<PathTriangle> Triangles => _triangles; 
        public PathTriangle MinTriangle => _minTriangle; 

        public PlanarFace GetRoomHorizontalFace() {
            Solid roomSolid = _room.ClosedShell
                .OfType<Solid>()
                .First();

            var faceArray = roomSolid.Faces;
            List<Face> faceList = new List<Face>();
            foreach(Face face in faceArray) { faceList.Add(face); }

            PlanarFace lowestFace = faceList
                .OfType<PlanarFace>()
                .Where(y => y.FaceNormal.Z != 0)
                .First();

            return lowestFace;
        }

        private List<PathTriangle> GetTriangleList() {
            var triangleList = new List<PathTriangle>();
            for(int i = 0; i < _mesh.NumTriangles; i++) {
                triangleList.Add(new PathTriangle(_mesh.get_Triangle(i)));
            }
            return triangleList;
        }

        private void FindNextTriangles() {
            foreach(PathTriangle triangle1 in _triangles) {
                foreach(PathTriangle triangle2 in _triangles) {
                    if(CheckIsNextTriangle(triangle1, triangle2)) {
                        triangle1.NextTriangles.Add(triangle2);
                    }
                }
            }
        }

        private PathTriangle GetMinTriangle() {
            UV minPoint = _roomBB.Min;
            double minDistance = double.MaxValue;
            double distance;

            PathTriangle minTriangle = null;

            foreach(var tri in _triangles) {
                foreach(var vertex in tri.Vertices) {
                    var checkPoint = new UV(vertex.X, vertex.Y);
                    distance = minPoint.DistanceTo(checkPoint);
                    if(distance < minDistance) {
                        minDistance = distance;
                        minTriangle = tri;
                    }
                }
            }

            return minTriangle;
        }

        private bool CheckIsNextTriangle(PathTriangle triangle1, PathTriangle triangle2) {
            int commonPoints = 0;

            List<XYZ> points1 = triangle1.Vertices;
            List<XYZ> points2 = triangle2.Vertices;

            foreach(XYZ point1 in points1) {
                foreach(XYZ point2 in points2) {
                    if(point1.X == point2.X && point1.Y == point2.Y) {
                        commonPoints++;
                    }
                }
            }

            // У соседних треугольников должны быть только две общие точки
            if(commonPoints == 2)
                return true;

            return false;
        }
    }
}
