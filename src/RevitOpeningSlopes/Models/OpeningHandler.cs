using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {
    internal class OpeningHandler {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyInstance _opening;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly NearestElements _nearestElements;
        private readonly SolidOperations _solidOperations;

        private Outline _outlineWithOffset;
        private BoundingBoxXYZ _openingBoundingBox;
        private Solid _nearestElementsSolid;
        private Solid _openingSolid;
        private XYZ _frontOffsetPoint;
        private XYZ _centralBackwardOffsetPoint;
        private XYZ _openingDepthPoint;
        private XYZ _centralOpeningDepthPoint;
        private XYZ _openingVector;
        private XYZ _openingBboxOrigin;
        private XYZ _rightPoint;
        private XYZ _rightFrontPoint;
        private XYZ _depthPoint;
        private XYZ _rightDepthPoint;
        private XYZ _horizontalCenterPoint;
        private XYZ _horizontalDepthPoint;
        private XYZ _topPoint;
        private XYZ _bottomPoint;
        private XYZ _verticalCenterPoint;

        private double _openingHeight = 0;
        private double _openingWidth = 0;
        private double _openingDepth = 0;
        private double _rotationAngle = 0;

        /// <summary>
        /// Шаг, на который делятся линии для поиска пересечений, ~10мм
        /// </summary>
        private const double _step = 0.032;

        /// <summary>
        /// Максимальная величина половины ширины окна, мм
        /// </summary>
        private const double _halfWidthLength = 3000;

        /// <summary>
        /// Длина линии, запускаемой вдоль окна для P7 и P8, мм
        /// </summary>
        private const double _alongsideLineLength = 300;

        /// <summary>
        /// Длина линий, запускаемых внутрь здания для P7 и P8, мм
        /// </summary>
        private const double _depthLineLength = 600;

        /// <summary>
        /// Максимальная ширина окна, мм
        /// </summary>
        private const double _widthLength = _halfWidthLength * 2;

        /// <summary>
        /// Длина линии, запущенной вверх, мм
        /// </summary>
        private const double _upperLineLength = 5000;

        /// <summary>
        /// Длина линии, запущенной вниз, мм
        /// </summary>
        private const double _bottomLineLength = 6000;

        public OpeningHandler(RevitRepository revitRepository, LinesFromOpening linesFromOpening,
            NearestElements nearestElements, SolidOperations solidOperations, FamilyInstance opening) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _linesFromOpening = linesFromOpening
                ?? throw new ArgumentNullException(nameof(linesFromOpening));
            _nearestElements = nearestElements
                ?? throw new ArgumentNullException(nameof(nearestElements));
            _solidOperations = solidOperations
                ?? throw new ArgumentNullException(nameof(solidOperations));
            _opening = opening
                ?? throw new ArgumentNullException(nameof(opening));

            // Запуск функции перерасчета направления вектора семейства окна
            RecalculationVectorDirection();
        }

        private XYZ GetOpeningVector() {
            if(_openingVector != null) {
                return _openingVector;
            } else {
                _openingVector = _revitRepository.GetOpeningVector(_opening);
                return _openingVector;
            }
        }

        /// <summary>
        /// Функция возвращает точку, расположенную в центре геометрического BoundingBox семейства окна
        /// </summary>
        /// <returns>Точка центра геометрического BoundingBox семейства окна</returns>
        public XYZ GetOpeningBoundingBoxOrigin() {
            if(_openingBboxOrigin != null) {
                return _openingBboxOrigin;
            } else {
                var openingBoundingBox = GetOpeningGeometryBbox();
                var minPoint = openingBoundingBox.Min;
                var maxPoint = openingBoundingBox.Max;

                double x = (minPoint.X + maxPoint.X) / 2;
                double y = (minPoint.Y + maxPoint.Y) / 2;
                double z = (minPoint.Z + maxPoint.Z) / 2;
                _openingBboxOrigin = new XYZ(x, y, z);

                return _openingBboxOrigin;
            }
        }

        /// <summary>
        /// Функция создает BoundingBox из объединенного Solid семейства окна
        /// </summary>
        /// <returns>BoundingBox из геометрии окна</returns>
        private BoundingBoxXYZ GetOpeningGeometryBbox() {
            if(_openingBoundingBox != null) {
                return _openingBoundingBox;
            } else {
                var openingSolid = GetOpeningSolid();
                if(openingSolid != null) {
                    var bbox = openingSolid.GetBoundingBox();
                    _openingBoundingBox = new BoundingBoxXYZ() {
                        Min = bbox.Transform.OfPoint(bbox.Min),
                        Max = bbox.Transform.OfPoint(bbox.Max)
                    };
                }
                return _openingBoundingBox;
            }
        }

        private Solid GetOpeningSolid() {
            if(_openingSolid != null) {
                return _openingSolid;
            } else {
                _openingSolid = _solidOperations.GetUnitedSolidFromOpening(_opening);
                return _openingSolid;
            }
        }


        /// <summary>
        /// Функция создает точку по центру семейства окна с отступом назад (P1)
        /// </summary>
        /// <returns>Центральная точка с отступом назад (P1)</returns>
        private XYZ GetCentralBackwardOffsetPoint() {
            if(_centralBackwardOffsetPoint != null) {
                return _centralBackwardOffsetPoint;
            } else {
                var openingBboxOrigin = GetOpeningBoundingBoxOrigin();
                var openingVector = GetOpeningVector();
                if(openingBboxOrigin != null && openingVector != null) {
                    const double backwardOffset = 500; // Длина отступа от центра окна внутрь здания, мм
                    _centralBackwardOffsetPoint = new XYZ(
                        openingBboxOrigin.X,
                        openingBboxOrigin.Y,
                        openingBboxOrigin.Z)
                        - openingVector * _revitRepository.ConvertToFeet(backwardOffset);
                }
                return _centralBackwardOffsetPoint;
            }
        }

        /// <summary>
        /// Функция создает центральную точку с отступом вперед (P2) из центральной точки с отступом назад (P1)
        /// </summary>
        /// <param name="frontLineLength">Длина отступа вперед</param>
        /// <returns>Центральная точка с отступом вперед (P2)</returns>
        private XYZ GetFrontOffsetPoint(double frontLineLength = 1500) {
            if(_frontOffsetPoint != null) {
                return _frontOffsetPoint;
            } else {
                var centralBackwardOffsetPoint = GetCentralBackwardOffsetPoint();
                var openingVector = GetOpeningVector();

                if(centralBackwardOffsetPoint != null && openingVector != null) {
                    _frontOffsetPoint = new XYZ(
                    centralBackwardOffsetPoint.X,
                    centralBackwardOffsetPoint.Y,
                    centralBackwardOffsetPoint.Z)
                    + openingVector * _revitRepository.ConvertToFeet(frontLineLength);
                }
                return _frontOffsetPoint;
            }
        }

        /// <summary>
        /// Создание объекта Outline - увеличенного BoundingBox из BoundingBox семейства окна 
        /// </summary>
        /// <returns></returns>
        private Outline GetOutlineWithOffset() {
            if(_outlineWithOffset != null) {
                return _outlineWithOffset;
            } else {
                var openingBoundingBox = GetOpeningGeometryBbox();
                if(openingBoundingBox != null) {
                    double offsetLength = _revitRepository.ConvertToFeet(800);
                    var minPoint = openingBoundingBox.Min - new XYZ(1, 1, 1) * offsetLength;
                    var maxPoint = openingBoundingBox.Max + new XYZ(1, 1, 1) * offsetLength;
                    _outlineWithOffset = new Outline(minPoint, maxPoint);
                }
                return _outlineWithOffset;
            }
        }

        /// <summary>
        /// Перерасчет вектора окна, размещенного в проекте и установка направления вектора окна в сторону от фасада
        /// </summary>
        private void RecalculationVectorDirection() {
            const double lineLength = 100; // Длина для направления вектора, мм
            var originPoint = GetOpeningBoundingBoxOrigin();
            var openingVector = GetOpeningVector();

            var forwardLine = _linesFromOpening.CreateLineFromOpening(
                originPoint,
                openingVector,
                lineLength,
                Direction.Forward);

            var backwardLine = _linesFromOpening.CreateLineFromOpening(
                originPoint,
                openingVector,
                lineLength,
                Direction.Backward);

            var forwardElements = _nearestElements.GetElementsByRay(forwardLine);
            var backwardElements = _nearestElements.GetElementsByRay(backwardLine);

            // Предполагается, что там, где луч пересек меньше элементов - направление от здания, иначе - направление
            // внутрь здания, которое нужно развернуть
            if(forwardElements.Count > backwardElements.Count) {
                _openingVector = _openingVector.Negate();
            }
        }

        /// <summary>
        /// Функция создает объединенный Solid из элементов, находящихся внутри увеличенного BoundingBox семейства окна
        /// </summary>
        /// <returns>Объединенный Solid из Solid элементов вокруг окна</returns>
        private Solid GetNearestElementsSolid() {
            if(_nearestElementsSolid != null) {
                return _nearestElementsSolid;
            } else {
                var outlineWithOffset = GetOutlineWithOffset();
                _nearestElementsSolid = _solidOperations.GetUnitedSolidFromBoundingBox(outlineWithOffset);
            }
            return _nearestElementsSolid;
        }

        private XYZ GetDirectionVectorFromOpening(Direction direction) {
            var openingVector = GetOpeningVector();
            var rightVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();

            switch(direction) {

                case Direction.Right:
                    return rightVector;

                case Direction.Left:
                    return -rightVector;

                case Direction.Forward:
                    return openingVector;

                case Direction.Backward:
                    return -openingVector;

                case Direction.Top:
                    return XYZ.BasisZ;

                case Direction.Down:
                    return -XYZ.BasisZ;

                default:
                    return XYZ.Zero;
            }
        }

        private IList<PlanarFace> GetFacesBySolidAndDirection(Solid solid, Direction direction) {
            var targetDirection = GetDirectionVectorFromOpening(direction);
            double tolerance = 0.99;
            var faceResultList = new List<PlanarFace>();

            foreach(Face face in solid.Faces) {

                var planarFace = face as PlanarFace;

                if(planarFace == null) {
                    continue;
                }

                var normal = planarFace.FaceNormal.Normalize();

                double dot = normal.DotProduct(targetDirection);

                if(dot >= tolerance) {
                    faceResultList.Add(planarFace);

                }

            }
            return faceResultList;
        }

        //public void CreateTestModelLine(Line geomLine) {
        //    var direction = geomLine.Direction.Normalize();

        //    var up = XYZ.BasisZ;

        //    // Если линия почти вертикальная
        //    if(Math.Abs(direction.DotProduct(up)) > 0.99) {
        //        up = XYZ.BasisX;
        //    }

        //    var normal = direction.CrossProduct(up).Normalize();

        //    var plane = Plane.CreateByNormalAndOrigin(
        //        normal,
        //        geomLine.Origin);

        //    var sketchPlane = SketchPlane.Create(
        //        _revitRepository.Document,
        //        plane);

        //    _revitRepository.Document.Create.NewModelCurve(
        //        geomLine,
        //        sketchPlane);
        //}

        public void CreateTestModelLine(Line geomLine, string lineStyleName = null) {
            var doc = _revitRepository.Document;
            var direction = geomLine.Direction.Normalize();
            var up = XYZ.BasisZ;
            if(System.Math.Abs(direction.DotProduct(up)) > 0.99) {
                up = XYZ.BasisX;
            }

            var normal = direction.CrossProduct(up).Normalize();
            var plane = Plane.CreateByNormalAndOrigin(normal, geomLine.Origin);
            var sketchPlane = SketchPlane.Create(doc, plane);
            var modelCurve = doc.Create.NewModelCurve(geomLine, sketchPlane);
            // Назначаем стиль линии по имени (если передан)
            if(!string.IsNullOrWhiteSpace(lineStyleName)) {
                var style = GetLineStyleByName(doc, lineStyleName);
                if(style != null) {
                    modelCurve.LineStyle = style;
                }
            }
        }
        private GraphicsStyle GetLineStyleByName(Document doc, string styleName) {
            var linesCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            if(linesCat == null) {
                return null;
            }

            foreach(Category subCat in linesCat.SubCategories) {
                if(!subCat.Name.Equals(styleName, System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                return subCat.GetGraphicsStyle(GraphicsStyleType.Projection);
            }
            return null;
        }

        //private XYZ GetClosestPointFromFacesByPoint(IList<PlanarFace> faces, XYZ targetPoint) {
        //    double minDistance = double.MaxValue;
        //    XYZ closestPoint = null;
        //    foreach(var face in faces) {
        //        var bb = face.GetBoundingBox();

        //        int steps = 5;

        //        for(int i = 0; i <= steps; i++) {
        //            for(int j = 0; j <= steps; j++) {
        //                double u = bb.Min.U + (bb.Max.U - bb.Min.U) * i / steps;
        //                double v = bb.Min.V + (bb.Max.V - bb.Min.V) * j / steps;

        //                var currentPoint = face.Evaluate(new UV(u, v));

        //                double currentLength = currentPoint.DistanceTo(targetPoint);

        //                if(currentLength < minDistance) {
        //                    minDistance = currentLength;
        //                    closestPoint = currentPoint;
        //                }
        //            }
        //        }
        //    }
        //    return closestPoint;
        //}

        private XYZ GetClosestPointFromTargetPoint(IList<XYZ> points, XYZ targetPoint) {
            double minDistance = double.MaxValue;
            XYZ closestPoint = null;
            foreach(var point in points) {
                double currentDistance = targetPoint.DistanceTo(point);
                if(currentDistance < minDistance) {
                    minDistance = currentDistance;
                    closestPoint = point;
                }
            }
            return closestPoint;
        }

        /// <summary>
        /// Оставляет "ближний кластер" точек относительно targetPoint.
        /// Идея: сортируем по расстоянию и ищем самый большой скачок дистанции.
        /// Всё до скачка оставляем, остальное отбрасываем.
        /// </summary>
        private List<XYZ> KeepNearestCluster(
            IEnumerable<XYZ> points,
            XYZ targetPoint,
            double gapRatioThreshold = 1.25) {
            if(points == null) {
                return new List<XYZ>();
            }

            var ordered = points
                .Select(p => new {
                    Point = p,
                    Distance = p.DistanceTo(targetPoint)
                })
                .OrderBy(x => x.Distance)
                .ToList();
            if(ordered.Count <= 1) {
                return ordered.Select(x => x.Point).ToList();
            }

            int splitIndex = -1;
            double maxRatio = 1.0;
            const double eps = 1e-9;
            for(int i = 0; i < ordered.Count - 1; i++) {
                double d1 = ordered[i].Distance;
                double d2 = ordered[i + 1].Distance;
                double ratio = d2 / Math.Max(d1, eps);
                if(ratio > maxRatio) {
                    maxRatio = ratio;
                    splitIndex = i;
                }
            }
            // Если явного скачка нет — считаем, что все точки "близкие"
            return splitIndex < 0 || maxRatio < gapRatioThreshold
                ? ordered.Select(x => x.Point).ToList()
                : ordered.Take(splitIndex + 1).Select(x => x.Point).ToList();
        }

        private XYZ GetClosestPointFromFacesByPoint(IList<PlanarFace> faces, Direction direction, bool closest = true) {

            var frontOffsetPoint = GetFrontOffsetPoint();
            IList<XYZ> closestPoints = new List<XYZ>();
            foreach(var face in faces) {
                var bb = face.GetBoundingBox();

                int steps = 3;
                XYZ closestPoint = null;
                double minDistance = double.MaxValue;
                for(int i = 0; i <= steps; i++) {
                    for(int j = 0; j <= steps; j++) {
                        double u = bb.Min.U + (bb.Max.U - bb.Min.U) * i / steps;
                        double v = bb.Min.V + (bb.Max.V - bb.Min.V) * j / steps;

                        var currentPoint = face.Evaluate(new UV(u, v));

                        double currentLength = currentPoint.DistanceTo(frontOffsetPoint);
                        if(currentLength < minDistance) {
                            minDistance = currentLength;
                            closestPoint = currentPoint;
                        }
                    }
                }
                closestPoints.Add(closestPoint);
            }
            if(direction == Direction.Top) {
                IList<XYZ> nearestPoints = KeepNearestCluster(closestPoints, frontOffsetPoint);
                IList<XYZ> resultPoints = new List<XYZ>();
                foreach(var point in nearestPoints) {
                    if(point.Z > frontOffsetPoint.Z) {
                        continue;
                    }
                    resultPoints.Add(point);
                }
                var result = closest ? resultPoints.OrderByDescending(p => p.Z).FirstOrDefault() : resultPoints.OrderBy(p => p.Z).FirstOrDefault();
                var testLine = Line.CreateBound(result, frontOffsetPoint);
                //CreateTestModelLine(testLine);
                return result;
            }
            if(direction == Direction.Down) {
                IList<XYZ> nearestPoints = KeepNearestCluster(closestPoints, frontOffsetPoint);
                IList<XYZ> resultPoints = new List<XYZ>();
                foreach(var point in nearestPoints) {
                    if(point.Z < frontOffsetPoint.Z) {
                        continue;
                    }
                    resultPoints.Add(point);
                }
                var result = closest ? resultPoints.OrderBy(p => p.Z).FirstOrDefault() : resultPoints.OrderByDescending(p => p.Z).FirstOrDefault();
                var testLine = Line.CreateBound(result, frontOffsetPoint);
                //CreateTestModelLine(testLine);
                return result;
            }
            if(direction == Direction.Left) {
                IList<XYZ> nearestPoints = KeepNearestCluster(closestPoints, frontOffsetPoint);
                var resultPoints = nearestPoints;
                //foreach(var point in nearestPoints) {
                //    if(point.Z < frontOffsetPoint.Z) {
                //        continue;
                //    }
                //    resultPoints.Add(point);
                //}
                var result = closest
                    ? resultPoints.OrderBy(p => p.DistanceTo(frontOffsetPoint)).FirstOrDefault()
                    : resultPoints.OrderByDescending(p => p.DistanceTo(frontOffsetPoint)).FirstOrDefault();
                var testLine = Line.CreateBound(result, frontOffsetPoint);
                //CreateTestModelLine(testLine);
                return result;
            }
            if(direction == Direction.Right) {
                IList<XYZ> nearestPoints = KeepNearestCluster(closestPoints, frontOffsetPoint);
                var resultPoints = nearestPoints;
                //foreach(var point in nearestPoints) {
                //    if(point.Z < frontOffsetPoint.Z) {
                //        continue;
                //    }
                //    resultPoints.Add(point);
                //}
                var result = closest
                    ? resultPoints.OrderBy(p => p.DistanceTo(frontOffsetPoint)).FirstOrDefault()
                    : resultPoints.OrderByDescending(p => p.DistanceTo(frontOffsetPoint)).FirstOrDefault();
                var testLine = Line.CreateBound(result, frontOffsetPoint);
                //CreateTestModelLine(testLine);
                return result;
            }
            return null;
        }


        public XYZ GetClosestPointByDirection(Direction direction) {
            var nearestElementsSolid = GetNearestElementsSolid();
            _revitRepository.Document.Regenerate();
            var frontOffsetPoint = GetFrontOffsetPoint();
            var facesList = GetFacesBySolidAndDirection(nearestElementsSolid, direction);

            var closestPoint = GetClosestPointFromFacesByPoint(facesList, direction);

            try {
                var testLine = Line.CreateBound(frontOffsetPoint, closestPoint);
                //CreateTestModelLine(testLine);

            } catch {
            }

            return closestPoint;
        }

        //public XYZ GetClosestOrthogonalPointByDirection(Direction direction) {
        //    var nearestElementsSolid = GetNearestElementsSolid();
        //    _revitRepository.Document.Regenerate();
        //    var frontOffsetPoint = GetFrontOffsetPoint();
        //    var facesList = GetFacesBySolidAndDirection(nearestElementsSolid, direction);

        //    var closestPoint = GetClosestPointFromFacesByPoint(facesList, direction);

        //    try {
        //        var testLine = Line.CreateBound(frontOffsetPoint, closestPoint);
        //        CreateTestModelLine(testLine);

        //    } catch {
        //    }

        //    return closestPoint;
        //}

        /// <summary>
        /// Функция находит точку, соответствующей самой выпирающей части окна (P3)
        /// </summary>
        /// <returns>Точка самой выпирающей части окна (P3)</returns>
        private XYZ GetOpeningDepthPoint() {
            if(_openingDepthPoint != null) {
                return _openingDepthPoint;
            } else {
                var centralBackwardOffsetPoint = GetCentralBackwardOffsetPoint();
                var frontOffsetPoint = GetFrontOffsetPoint();
                if(centralBackwardOffsetPoint != null && frontOffsetPoint != null) {
                    double closestDist = double.PositiveInfinity;
                    double backWardLength = _revitRepository.ConvertToMillimeters(
                        centralBackwardOffsetPoint
                        .DistanceTo(frontOffsetPoint));

                    var openingVector = GetOpeningVector();

                    // Создание линии вправо и разделения ее на точки для запуска массива линий из этих точек
                    // в сторону окна
                    var rightLine = _linesFromOpening.CreateLineFromOpening(
                        frontOffsetPoint,
                        openingVector,
                        _halfWidthLength,
                        Direction.Right);

                    var points = _linesFromOpening.SplitCurveToPoints(rightLine, _step);
                    var openingSolid = GetOpeningSolid();

                    foreach(var point in points) {
                        var backWardLine = _linesFromOpening.CreateLineFromOpening(
                            point,
                            openingVector,
                            backWardLength,
                            Direction.Backward);

                        var intersectOptOutside = new SolidCurveIntersectionOptions() {
                            ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                        };
                        var intersection = openingSolid.IntersectWithCurve(
                            backWardLine,
                            intersectOptOutside);

                        if(intersection.SegmentCount > 0) {
                            var intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                            double currentDist = point.DistanceTo(intersectCoord);
                            if(currentDist < closestDist) {
                                closestDist = currentDist;

                                // Назначаем полю последнюю ближайшую точку пересечения к точке, откуда была запущена
                                // линия
                                _openingDepthPoint = intersectCoord;
                            }
                        }
                    }
                }
                return _openingDepthPoint;
            }
        }

        /// <summary>
        /// Функция запускает 2 линии - в направлении внутрь здания из точки (P2) и влево из точки
        /// окна (P3) и возвращает точку пересечения этих двух линий (P4)
        /// </summary>
        /// <returns>Центральная самая отдаленная точка фасада (P4)</returns>
        private XYZ GetCentralOpeningDepthPoint() {
            if(_centralOpeningDepthPoint != null) {
                return _centralOpeningDepthPoint;
            } else {
                var centralBackwardOffsetPoint = GetCentralBackwardOffsetPoint();
                var frontOffsetPoint = GetFrontOffsetPoint();
                var openingDepthPoint = GetOpeningDepthPoint();
                var openingVector = GetOpeningVector();

                if(centralBackwardOffsetPoint != null && frontOffsetPoint != null && openingDepthPoint != null) {
                    double backWardLength = _revitRepository.ConvertToMillimeters(
                        centralBackwardOffsetPoint
                        .DistanceTo(frontOffsetPoint));

                    // Создание линий влево из точки P3 и в сторону окна из центральной точки P2
                    var lineFromDepthPointToLeft = _linesFromOpening.CreateLineFromOpening(
                        openingDepthPoint,
                        openingVector,
                        _halfWidthLength,
                        Direction.Left);

                    var backwardLineFromOffsetPoint = _linesFromOpening.CreateLineFromOpening(
                        frontOffsetPoint,
                        openingVector,
                        backWardLength,
                        Direction.Backward);

                    IList<ClosestPointsPairBetweenTwoCurves> closestPoints =
                        new List<ClosestPointsPairBetweenTwoCurves>();

                    backwardLineFromOffsetPoint.ComputeClosestPoints(
                        lineFromDepthPointToLeft,
                        true,
                        false,
                        false,
                        out closestPoints);
                    _centralOpeningDepthPoint = closestPoints.First().XYZPointOnFirstCurve;
                }
                return _centralOpeningDepthPoint;
            }
        }

        /// <summary>
        /// Функция находит ближайшую правую точку к центру семейства окна (P5)
        /// </summary>
        /// <returns>Ближайшая правая точка проема к центру семейства окна (P5)</returns>
        /// <exception cref="ArgumentException">Срабатывает, если не найдена граница окна справа</exception>
        private XYZ GetRightPoint() {
            if(_rightPoint != null) {
                return _rightPoint;
            } else {
                var centralOpeningDepthPoint = GetCentralOpeningDepthPoint();
                var frontOffsetPoint = GetFrontOffsetPoint();
                var nearestElementsSolid = GetNearestElementsSolid();
                var openingVector = GetOpeningVector();

                if(centralOpeningDepthPoint != null && frontOffsetPoint != null && nearestElementsSolid != null) {
                    double closestDist = double.PositiveInfinity;

                    // Создание линии из точки P2 и P4 и разделение ее на точки для запуска линий вправо от окна
                    var forwardLine = Line.CreateBound(frontOffsetPoint, centralOpeningDepthPoint);
                    var points = _linesFromOpening.SplitCurveToPoints(forwardLine, _step);
                    foreach(var point in points) {
                        var rightLine = _linesFromOpening.CreateLineFromOpening(point, openingVector,
                            _halfWidthLength,
                            Direction.Right);

                        var intersectOptInside = new SolidCurveIntersectionOptions() {
                            ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                        };
                        var intersection = nearestElementsSolid.IntersectWithCurve(
                            rightLine,
                            intersectOptInside);
                        if(intersection.SegmentCount > 0) {
                            var intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(0);
                            double currentDist = _revitRepository
                                .ConvertToMillimeters(point.DistanceTo(intersectCoord));

                            if(currentDist < closestDist) {
                                closestDist = currentDist;

                                // Назначаем полю последнюю ближайшую точку пересечения к точке, откуда была запущена
                                // линия
                                _rightPoint = intersectCoord;
                            }
                        }
                    }
                }
                return _rightPoint ?? throw new ArgumentException("Окно не углублено внутрь фасада, либо отсутствует граница справа");
            }
        }

        /// <summary>
        /// Функция находит правую точку, расположенную на внешней стороне фасада, используя правую точку (P5)
        /// </summary>
        /// <returns>Правая точка на внешней стороне фасада (P6)</returns>
        private XYZ GetRightFrontPoint() {
            if(_rightFrontPoint != null) {
                return _rightFrontPoint;
            } else {
                var rightPoint = GetRightPoint();
                var openingVector = GetOpeningVector();
                var nearestElementsSolid = GetNearestElementsSolid();
                if(rightPoint != null && openingVector != null & nearestElementsSolid != null) {
                    const double backLineLength = 1000; // Длина линии, запускаемой назад, мм
                    const double pointForwardOffset = 800; // Длина отступа от точки P5 вперед, мм

                    var rightPointWithForwardOffset = rightPoint + openingVector
                        * _revitRepository.ConvertToFeet(pointForwardOffset);
                    var backLine = _linesFromOpening.CreateLineFromOpening(
                        rightPointWithForwardOffset,
                        openingVector,
                        backLineLength,
                        Direction.Backward);

                    var intersectOptInside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                    };
                    var intersection = nearestElementsSolid.IntersectWithCurve(
                        backLine,
                        intersectOptInside);
                    if(intersection.SegmentCount > 0) {
                        _rightFrontPoint = intersection.GetCurveSegment(0).GetEndPoint(0);
                    }
                }
                return _rightFrontPoint;
            }
        }

        /// <summary>
        /// Функция находит точку глубины (P7) - ближайшая точка в выпирающей геометрической части семейства 
        /// окна в сторону от здания
        /// </summary>
        /// <returns>Точка глубины (P7)</returns>
        private XYZ GetDepthPoint() {
            if(_depthPoint != null) {
                return _depthPoint;
            } else {
                var rightFrontPoint = GetRightFrontPoint();
                var openingSolid = GetOpeningSolid();
                var openingVector = GetOpeningVector();

                if(rightFrontPoint != null & openingSolid != null) {
                    double closestDist = double.PositiveInfinity;

                    // Создание линии влево от точки P6 и разделение ее на точки для создания линий в направлении внутрь
                    // здания
                    var alongsideLine = _linesFromOpening.CreateLineFromOpening(
                        rightFrontPoint,
                        openingVector,
                        _alongsideLineLength,
                        Direction.Left);
                    var points = _linesFromOpening.SplitCurveToPoints(alongsideLine, _step);
                    var intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };

                    foreach(var point in points) {
                        var backwardLine = _linesFromOpening.CreateLineFromOpening(
                            point,
                            openingVector,
                            _depthLineLength,
                            Direction.Backward);

                        var intersection = openingSolid.IntersectWithCurve(
                                backwardLine,
                                intersectOptOutside);

                        if(intersection.SegmentCount > 0) {
                            var intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                            double currentDist = _revitRepository
                                .ConvertToMillimeters(point.DistanceTo(intersectCoord));
                            if(currentDist < closestDist) {
                                closestDist = currentDist;

                                // Назначаем полю последнюю ближайшую точку пересечения к точке, откуда была запущена
                                // линия
                                _depthPoint = intersectCoord;
                            }
                        }
                    }
                }
                return _depthPoint;
            }
        }

        /// <summary>
        /// Функция находит правую точку глубины (P8), путем нахождения точки пересечения линий, запущенных 
        /// из точки P7 и P6
        /// </summary>
        /// <returns>Правая точка глубины (P8)</returns>
        private XYZ GetRightDepthPoint() {
            if(_rightDepthPoint != null) {
                return _rightDepthPoint;
            } else {
                var depthPoint = GetDepthPoint();
                var rightFrontPoint = GetRightFrontPoint();
                var openingVector = GetOpeningVector();

                if(depthPoint != null && rightFrontPoint != null) {

                    // Создание линий из точки внешней границы фасада P6 в направлении внутрь здания и из точки
                    // глубины P7 вправо для нахождения точки пересечения между ними - P8
                    var depthLine = _linesFromOpening.CreateLineFromOpening(
                        rightFrontPoint,
                        openingVector,
                        _depthLineLength,
                        Direction.Backward);

                    var lineFromClosestPoint = _linesFromOpening.CreateLineFromOpening(
                        depthPoint,
                        openingVector,
                        _alongsideLineLength,
                        Direction.Right);

                    IList<ClosestPointsPairBetweenTwoCurves> closestPoints =
                        new List<ClosestPointsPairBetweenTwoCurves>();

                    depthLine.ComputeClosestPoints(lineFromClosestPoint,
                        true,
                        false,
                        false,
                        out closestPoints);
                    _rightDepthPoint = closestPoints.First().XYZPointOnFirstCurve;
                }
                return _rightDepthPoint;
            }
        }

        /// <summary>
        /// Функция находит точку центра по горизонтали семейства окна (P9), используя точку P6
        /// </summary>
        /// <returns>Точка горизонтального центра P9</returns>
        private XYZ GetHorizontalCenterPoint() {
            if(_horizontalCenterPoint != null) {
                return _horizontalCenterPoint;
            } else {
                var rightFrontpoint = GetRightFrontPoint();
                var openingVector = GetOpeningVector();
                var nearestElementsSolid = GetNearestElementsSolid();

                if(rightFrontpoint != null && openingVector != null && nearestElementsSolid != null) {

                    var normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();

                    // Изменение стартовой точки для того, чтобы не было пересечения со стеной 
                    // справа у запущенной линии
                    var startPoint = new XYZ(
                        rightFrontpoint.X,
                        rightFrontpoint.Y,
                        rightFrontpoint.Z) - normalVector * _step;

                    var leftLine = _linesFromOpening.CreateLineFromOpening(
                        startPoint,
                        openingVector,
                        _widthLength,
                        Direction.Left);

                    var intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    var intersection = nearestElementsSolid.IntersectWithCurve(
                        leftLine,
                        intersectOptOutside);

                    if(intersection.SegmentCount > 0) {
                        var intersectCoord = intersection.GetCurveSegment(0).GetEndPoint(1);
                        _horizontalCenterPoint = new XYZ((
                            intersectCoord.X + rightFrontpoint.X) / 2,
                            (intersectCoord.Y + rightFrontpoint.Y) / 2,
                            intersectCoord.Z);
                    }
                }
                return _horizontalCenterPoint;
            }
        }

        /// <summary>
        /// Функция находит горизонтальный центр проема в плоскости вставки семейства откоса (P10), используя 
        /// точку P9 и P8
        /// </summary>
        /// <returns>Точка горизонтального центра в плоскости вставки семейства откоса P10</returns>
        private XYZ GetHorizontalDepthPoint() {
            if(_horizontalDepthPoint != null) {
                return _horizontalDepthPoint;
            } else {
                var horizontalCenterPoint = GetHorizontalCenterPoint();
                var rightDepthPoint = GetRightDepthPoint();
                var openingVector = GetOpeningVector();

                if(horizontalCenterPoint != null && rightDepthPoint != null) {
                    const double depthLength = 800; // Длина линии, запущенной внутрь здания из точки P9


                    var lineFromHorizontalPoint = _linesFromOpening.CreateLineFromOpening(
                        horizontalCenterPoint,
                        openingVector,
                        depthLength,
                        Direction.Backward);

                    var lineFromRightPoint = _linesFromOpening.CreateLineFromOpening(
                        rightDepthPoint,
                        openingVector,
                        _halfWidthLength,
                        Direction.Left);

                    IList<ClosestPointsPairBetweenTwoCurves> closestPoints =
                        new List<ClosestPointsPairBetweenTwoCurves>();

                    lineFromHorizontalPoint.ComputeClosestPoints(
                        lineFromRightPoint,
                        true,
                        false,
                        false,
                        out closestPoints);

                    _horizontalDepthPoint = closestPoints.First().XYZPointOnFirstCurve;
                }
                return _horizontalDepthPoint;
            }
        }

        /// <summary>
        /// Функция находит верхнюю точку проема (P11), используя точку P10
        /// </summary>
        /// <returns>Верхняя точка проема P11</returns>
        private XYZ GetTopPoint() {
            if(_topPoint != null) {
                return _topPoint;
            } else {
                var horizontalDepthPoint = GetHorizontalDepthPoint();
                var nearestElementsSolid = GetNearestElementsSolid();
                var openingVector = GetOpeningVector();

                if(horizontalDepthPoint != null && nearestElementsSolid != null) {
                    const double forwardLineLength = 1000;
                    double closestDist = double.PositiveInfinity;

                    // Создание линии из точки горизонтального центра P10 в направлении от фасада и разделение
                    // ее на точки для построения линий в направлении наверх
                    var forwardLine = _linesFromOpening.CreateLineFromOpening(
                        horizontalDepthPoint,
                        openingVector,
                        forwardLineLength,
                        Direction.Forward);

                    var points = _linesFromOpening.SplitCurveToPoints(forwardLine, _step);
                    var intersectOptInside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                    };

                    foreach(var point in points) {
                        var upwardLine = _linesFromOpening.CreateLineFromOpening(
                            point,
                            openingVector,
                            _upperLineLength,
                            Direction.Top);
                        var topIntersection = nearestElementsSolid.IntersectWithCurve(
                            upwardLine,
                            intersectOptInside);

                        if(topIntersection.SegmentCount > 0) {
                            var intersectCoord = topIntersection.GetCurveSegment(0).GetEndPoint(0);
                            double currentDist = _revitRepository
                                .ConvertToMillimeters(point.DistanceTo(intersectCoord));
                            if(currentDist < closestDist) {
                                closestDist = currentDist;

                                // Назначаем полю последнюю ближайшую точку пересечения к точке, откуда была запущена
                                // линия
                                _topPoint = intersectCoord;
                            }
                        }
                    }
                }
                return _topPoint;
            }
        }

        /// <summary>
        /// Функция находит нижнюю точку проема (P12)
        /// </summary>
        /// <returns>Нижняя точка проема P12</returns>
        private XYZ GetBottomPoint() {
            if(_bottomPoint != null) {
                return _bottomPoint;
            } else {
                var horizontalDepthPoint = GetHorizontalDepthPoint();
                var nearestElementsSolid = GetNearestElementsSolid();
                var openingVector = GetOpeningVector();

                if(horizontalDepthPoint != null && nearestElementsSolid != null) {

                    var bottomLine = _linesFromOpening.CreateLineFromOpening(
                        horizontalDepthPoint,
                        openingVector,
                        _bottomLineLength,
                        Direction.Down);

                    var intersectOptOutside = new SolidCurveIntersectionOptions() {
                        ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                    };
                    var bottomIntersection = nearestElementsSolid.IntersectWithCurve(
                        bottomLine,
                        intersectOptOutside);

                    if(bottomIntersection.SegmentCount > 0) {
                        _bottomPoint = bottomIntersection.GetCurveSegment(0).GetEndPoint(1);
                    }
                }
                return _bottomPoint;
            }
        }

        /// <summary>
        /// Реализация скалярного произведения двух векторов
        /// </summary>
        /// <param name="originVector"></param>
        /// <param name="openingVector"></param>
        /// <returns>Скалярное произведение</returns>
        private double ScalarMultiply(XYZ originVector, XYZ openingVector) {
            return originVector != null && openingVector != null ? openingVector.X * originVector.X + openingVector.Y * originVector.Y : 0;
        }

        /// <summary>
        /// Функция вычисляет длину вектора в двумерном пространстве
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Значение длины вектора</returns>
        private double Magnitude(double x, double y) {
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Функция находит точку центра проема по вертикали (P13), используя верхнюю (P11) и нижнюю (P12) 
        /// точку соответственно
        /// </summary>
        /// <returns>Точка центра проема по вертикали P13</returns>
        public XYZ GetVerticalCenterPoint() {
            if(_verticalCenterPoint != null) {
                return _verticalCenterPoint;
            } else {
                var topPoint = GetTopPoint();
                var bottomPoint = GetBottomPoint();

                if(topPoint != null && bottomPoint != null) {
                    double x = bottomPoint.X;
                    double y = bottomPoint.Y;
                    double z = (topPoint.Z + bottomPoint.Z) / 2;
                    _verticalCenterPoint = new XYZ(x, y, z);
                }
                return _verticalCenterPoint;
            }
        }

        /// <summary>
        /// Функция находит высоту проема, используя верхнюю (P11) и нижнюю (P12) 
        /// точку соответственно
        /// </summary>
        /// <returns>Высота проема</returns>
        /// <exception cref="ArgumentException">Срабатывает, если не удалось рассчитать верхнюю 
        /// и нижнюю точку окна</exception>
        public double GetOpeningHeight() {
            var topPoint = GetTopPoint();
            var bottomPoint = GetBottomPoint();

            _openingHeight = topPoint != null && bottomPoint != null
                ? Math.Abs(bottomPoint.Z - topPoint.Z)
                : throw new ArgumentException("Не удалось рассчитать высоту проема");
            return _openingHeight;
        }

        /// <summary>
        /// Функция находит ширину проема, используя точки P13 и P8
        /// </summary>
        /// <returns>Ширина проема</returns>
        /// <exception cref="ArgumentException">Срабатывает, если не была найдена точка справа и 
        /// вертикальный центр окна</exception>
        public double GetOpeningWidth() {
            var verticalCenterPoint = GetVerticalCenterPoint();
            var rightDepthPoint = GetRightDepthPoint();

            _openingWidth = verticalCenterPoint != null && rightDepthPoint != null
                ? Math.Sqrt(Math.Pow(verticalCenterPoint.X - rightDepthPoint.X, 2)
                    + Math.Pow(verticalCenterPoint.Y - rightDepthPoint.Y, 2)) * 2
                : throw new ArgumentException("не удалось рассчитать ширину проема");
            return _openingWidth;
        }

        /// <summary>
        /// Функция находит глубину проема, используя точки P8 и P6
        /// </summary>
        /// <returns>Глубина проема</returns>
        /// <exception cref="ArgumentException">Срабатывает, если не удалось рассчитать точку глубины и 
        /// вылета окна справа</exception>
        public double GetOpeningDepth() {

            var rightDepthPoint = GetRightDepthPoint();
            var rightFrontPoint = GetRightFrontPoint();

            _openingDepth = rightDepthPoint != null && rightFrontPoint != null
                ? rightDepthPoint.DistanceTo(rightFrontPoint)
                : throw new ArgumentException("Не удалось рассчитать глубину проема");
            return _openingDepth;
        }

        /// <summary>
        /// Функция находит угол поворота семейства окна
        /// </summary>
        /// <returns>Угол поворота для откоса, в радианах</returns>
        public double GetRotationAngle() {
            var openingVector = GetOpeningVector();

            if(openingVector != null) {
                var originVector = new XYZ(0, -1, 0);
                double scalarMultiply = ScalarMultiply(originVector, openingVector);
                double originMagnitude = Magnitude(originVector.X, originVector.Y);
                double openingMagnitude = Magnitude(openingVector.X, openingVector.Y);
                double cosTheta = scalarMultiply / (originMagnitude * openingMagnitude);

                _rotationAngle = Math.Acos(cosTheta);
                return openingVector.X < 0 ? -_rotationAngle : _rotationAngle;
            }
            return _rotationAngle;
        }
    }
}
