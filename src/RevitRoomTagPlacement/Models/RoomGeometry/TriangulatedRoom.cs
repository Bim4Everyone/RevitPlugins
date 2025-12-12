using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomTagPlacement.Models;
internal class TriangulatedRoom {
    private readonly Room _room;
    private readonly Mesh _mesh;
    private readonly BoundingBoxUV _roomBB;

    public TriangulatedRoom(Room room) {
        _room = room;
        var horizontalFace = GetRoomHorizontalFace();
        _mesh = horizontalFace.Triangulate();
        _roomBB = horizontalFace.GetBoundingBox();
        Triangles = GetTriangleList();
        MinTriangle = GetMinTriangle();
        FindNextTriangles();
    }

    public List<PathTriangle> Triangles { get; }
    public PathTriangle MinTriangle { get; }

    public PlanarFace GetRoomHorizontalFace() {
        var roomSolid = _room.ClosedShell
            .OfType<Solid>()
            .First();

        return roomSolid.Faces
            .OfType<PlanarFace>()
            .Where(y => y.FaceNormal.Z != 0)
            .First();
    }

    private List<PathTriangle> GetTriangleList() {
        var triangleList = new List<PathTriangle>();
        for(int i = 0; i < _mesh.NumTriangles; i++) {
            triangleList.Add(new PathTriangle(_mesh.get_Triangle(i)));
        }
        return triangleList;
    }

    private void FindNextTriangles() {
        foreach(var triangle1 in Triangles) {
            foreach(var triangle2 in Triangles) {
                if(CheckIsNextTriangle(triangle1, triangle2)) {
                    triangle1.NextTriangles.Add(triangle2);
                }
            }
        }
    }

    private PathTriangle GetMinTriangle() {
        var minPoint = _roomBB.Min;
        double minDistance = double.MaxValue;
        double distance;

        PathTriangle minTriangle = null;

        foreach(var tri in Triangles) {
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

        var points1 = triangle1.Vertices;
        var points2 = triangle2.Vertices;

        foreach(var point1 in points1) {
            foreach(var point2 in points2) {
                if(CompareDouble(point1.X, point2.X) && CompareDouble(point1.Y, point2.Y)) {
                    commonPoints++;
                }
            }
        }

        // У соседних треугольников должны быть только две общие точки
        return commonPoints == 2;
    }

    private static bool CompareDouble(double a, double b) {
        return Math.Abs(a - b) < 1e-9;
    }
}
