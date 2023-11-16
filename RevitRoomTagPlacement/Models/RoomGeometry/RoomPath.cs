using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomTagPlacement.Models {
    internal class RoomPath {
        private readonly TriangulatedRoom _triRoom;

        private readonly List<PathTriangle> _triangles;
        private readonly List<List<PathTriangle>> _startTriangles;
        private readonly List<List<PathTriangle>> _allPathes;
        private readonly List<PathTriangle> _mainPath;
        private readonly double _centerPoint;

        private readonly UV _tagPoint;

        public RoomPath(TriangulatedRoom triRoom) {
            _triRoom = triRoom;

            _triangles = this._triRoom.Triangles;            
            _startTriangles = GetStartEndTriangles();
            _allPathes = new List<List<PathTriangle>>();
            GetAllPathes();
            _mainPath = GetMainPath();
            _centerPoint = CalculateWeightCenter();

            _tagPoint = GetTagPointBySpline();
        }

        public UV TagPoint => _tagPoint;

        private UV GetTagPointBySpline() {
            if(_triangles.Count == 1) {
                return new UV(_triangles[0].Center.X, _triangles[0].Center.Y);
            } 
            else {
                List<XYZ> splinePoints = _mainPath.Select(x => x.Center).ToList();
                HermiteSpline spline = HermiteSpline.Create(splinePoints, false);
                XYZ point = spline.Evaluate(_centerPoint, true);

                return new UV(point.X, point.Y);
            }
        }

        private double CalculateWeightCenter() {
            double triWeightSum = 0;
            double weightSum = 0;

            for(int i = 0; i < _mainPath.Count; i++) {
                triWeightSum += i * _mainPath[i].Weight;
                weightSum += _mainPath[i].Weight;
            }

            double weightCenter = triWeightSum / weightSum;
            return weightCenter / (_mainPath.Count - 1);
        }

        private List<PathTriangle> GetMainPath() {
            List<PathTriangle> mainPath = _allPathes[0];
            double mainPathWeight = 0;

            foreach(var path in _allPathes) {
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

        private List<List<PathTriangle>> GetStartEndTriangles() {
            List<List<PathTriangle>> triangleList = new List<List<PathTriangle>>();

            List<PathTriangle> starts = _triangles
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
            if(_startTriangles.Count != 0) {
                foreach(var startCouple in _startTriangles) {
                    GoFromStartToEnd(new List<PathTriangle>(), startCouple[0], startCouple[1]);
                    foreach(var tri in _triangles) {
                        tri.IsVisited = false;
                    }
                }
            } 
            else {
                PathTriangle minTriangle = _triRoom.MinTriangle;
                GoFromStart(new List<PathTriangle>(), minTriangle);
            }
        }

        private void GoFromStartToEnd(List<PathTriangle> path, PathTriangle startTriangle, PathTriangle endTriangle) {
            startTriangle.IsVisited = true;
            path.Add(startTriangle);

            foreach(PathTriangle nextTriangle in startTriangle.NextTriangles) {
                List<PathTriangle> newPath = new List<PathTriangle>(path);
                if(!nextTriangle.IsVisited) { 
                    if(nextTriangle == endTriangle) {
                        newPath.Add(nextTriangle);
                        _allPathes.Add(newPath);
                    } 
                    else {
                        GoFromStartToEnd(newPath, nextTriangle, endTriangle);
                    }
                }
            }
        }

        private void GoFromStart(List<PathTriangle> path, PathTriangle startTriangle) {
            startTriangle.IsVisited = true;
            path.Add(startTriangle);
            List<PathTriangle> newPath = new List<PathTriangle>();

            foreach(PathTriangle nextTriangle in startTriangle.NextTriangles) {
                newPath = new List<PathTriangle> (path);
                if(!nextTriangle.IsVisited) { 
                    GoFromStart(newPath, nextTriangle);
                }
            }
            _allPathes.Add(newPath);
        }
    }
}
