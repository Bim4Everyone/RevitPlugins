using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomTagPlacement.Models {
    internal class RoomPath {
        private Room room;
        private Mesh mesh;
        private List<PathTriangle> triangles;
        private BoundingBoxUV roomBB;
        private List<List<PathTriangle>> startTriangles;
        private List<List<PathTriangle>> allPathes;
        private List<PathTriangle> mainPath;
        private double centerPoint;
        public UV TagPoint;

        public RoomPath(Room _room) {
            room = _room;
            mesh = GetRoomMesh();
            triangles = GetTriangleList();
            
            FindNextTriangles();
            startTriangles = GetStartEndTriangles();

            allPathes = new List<List<PathTriangle>>();
            GetAllPathes();

            mainPath = GetMainPath();

            centerPoint = CalculateWeightCenter();
            TagPoint = GetTagPoint();
        }

        private UV GetTagPoint() {
            double maxWeight = 0;
            List<PathTriangle> triPair = new List<PathTriangle>();

            foreach(var tri in triangles) {
                foreach(var nextTri in tri.NextTriangles) { 
                    double weight = tri.Weight + nextTri.Weight;
                    if(weight > maxWeight) {
                        triPair.Add(tri);
                        triPair.Add(nextTri);
                    }
                }
            }

            XYZ center1 = triPair[0].Center;
            XYZ center2 = triPair[1].Center;

            return new UV(
                (center1.X + center2.X) * 0.5,
                (center1.Y + center2.Y) * 0.5);
        }

        private double CalculateWeightCenter() {
            double triWeightSum = 0;
            double weightSum = 0;

            for(int i = 0; i < mainPath.Count; i++) {
                triWeightSum += i * mainPath[i].Weight;
                weightSum += mainPath[i].Weight;
            }

            double weightCenter = triWeightSum / weightSum;
            return weightCenter / (mainPath.Count - 1);
        }

        private List<PathTriangle> GetMainPath() {
            List<PathTriangle> mainPath = allPathes[0];
            double mainPathWeight = 0;

            foreach(var path in allPathes) {
                double pathWeight = 0;
                foreach(var triangle in path) {
                    pathWeight += triangle.Weight;
                }

                if(pathWeight > mainPathWeight) { 
                    mainPathWeight = pathWeight;
                    mainPath = path;
                }
            }
            return mainPath;
        }

        private Mesh GetRoomMesh() {
            Solid roomSolid = room.ClosedShell
                .OfType<Solid>()
                .First();

            var faceArray = roomSolid.Faces;

            List<Face> faceList = new List<Face>();
            foreach(Face face in faceArray) {
                faceList.Add(face);
            }

            PlanarFace lowestFace = faceList
                .OfType<PlanarFace>()
                .Where(y => y.FaceNormal.Z != 0)
                .First();

            roomBB = lowestFace.GetBoundingBox();

            return lowestFace
                .Triangulate();
        }

        private List<PathTriangle> GetTriangleList() { 
            var triangleList = new List<PathTriangle>();
            for(int i=0; i < mesh.NumTriangles; i++) {
                triangleList.Add(new PathTriangle(mesh.get_Triangle(i)));
            }
            return triangleList;        
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

            if(commonPoints == 2) return true;

            return false;
        }

        private void FindNextTriangles() {
            foreach(PathTriangle triangle1 in triangles) {
                foreach(PathTriangle triangle2 in triangles) {
                    if(CheckIsNextTriangle(triangle1, triangle2)) {
                        triangle1.NextTriangles.Add(triangle2);
                    }
                }
            }
        }

        private List<List<PathTriangle>> GetStartEndTriangles() {
            List<List<PathTriangle>> triangleList = new List<List<PathTriangle>>();

            List<PathTriangle> starts = triangles
                .Where(x => x.NextTriangles.Count == 1)
                .ToList();

            int length = starts.Count;

            for(int i = 0; i < length; i++) {
                for(int j = i + 1; j < length; j++) {
                    triangleList.Add(new List<PathTriangle> { starts[i], starts[j] }); 
                }
            }
            return triangleList;
        }

        private void GetAllPathes() {
            if(startTriangles.Count != 0) {
                foreach(var startCouple in startTriangles) {
                    GoFromStartToEnd(new List<PathTriangle>(), startCouple[0], startCouple[1]);
                    foreach(var tri in triangles) {
                        tri.IsViisted = false;
                     }
                }
            } 
            else {
                PathTriangle minTriangle = GetMinTriangle();
                GoFromStart(new List<PathTriangle>(), minTriangle);
            }
        }

        private PathTriangle GetMinTriangle() {
            UV minPoint = roomBB.Min;
            double minDistance = Double.MaxValue;

            PathTriangle minTriangle = null;

            foreach(var tri in triangles) {
                foreach(var vertex in tri.Vertices) {
                    var checkPoint = new UV(vertex.X, vertex.Y);
                    double distance = minPoint.DistanceTo(checkPoint);
                    if(distance < minDistance) {
                        minDistance = distance;
                        minTriangle = tri;
                    }
                }
            }

            return minTriangle;
        }

        private void GoFromStartToEnd(List<PathTriangle> path, PathTriangle startTriangle, PathTriangle endTriangle) {
            startTriangle.IsViisted = true;
            path.Add(startTriangle);

            foreach(PathTriangle nextTriangle in startTriangle.NextTriangles) {
                List<PathTriangle> newPath = new List<PathTriangle>(path);
                if(!nextTriangle.IsViisted) { 
                    if(nextTriangle == endTriangle) {
                        newPath.Add(nextTriangle);
                        allPathes.Add(newPath);
                    } 
                    else {
                        GoFromStartToEnd(newPath, nextTriangle, endTriangle);
                    }
                }
            }
        }

        private void GoFromStart(List<PathTriangle> path, PathTriangle startTriangle) {
            startTriangle.IsViisted = true;
            path.Add(startTriangle);
            List<PathTriangle> newPath = new List<PathTriangle>();

            foreach(PathTriangle nextTriangle in startTriangle.NextTriangles) {
                newPath = new List<PathTriangle> (path);
                if(!nextTriangle.IsViisted) { 
                    GoFromStart(newPath, nextTriangle);
                }
            }
            allPathes.Add(newPath);
        }
    }
}
