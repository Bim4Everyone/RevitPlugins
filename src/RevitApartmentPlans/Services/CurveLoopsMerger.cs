using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    /// <summary>
    /// Реализация алгоритма по объединению нескольких замкнутых плоских контуров в один плоский внешний контур.
    /// При этом изначальные контуры могут не пересекаться.
    /// <br/>
    /// Алгоритм объединения:<br/>
    /// 1. Построить копии всех контуров в плоскости XOY.<br/>
    /// 2. Выдавить солиды по каждому контуру.<br/>
    /// 3. Попытаться объединить получившиеся солиды в один солид.<br/>
    /// 4. Если солиды объединились в один солид, выполняем следующий шаг, если не объединились,
    /// увеличиваем каждый контур на заданный шаг оффсета и повторяем всё начиная с выдавливания солидов.<br/>
    /// Чтобы количество итераций имело разумный предел, необходимо установить максимальное значение оффсета.<br/>
    /// 5. У объединенного солида находим нижнюю грань (Face).<br/>
    /// 6. У этой нижней грани берем наружный контур (CurveLoop).<br/>
    /// 7. Делаем оффсет полученного контура в обратном направлении (уменьшаем его) на значение,
    /// на которое были увеличены контуры в последней итерации.<br/>
    /// 8. Поднимаем полученный наружный объединенный контур на Z координату первого исходного контура.
    /// </summary>
    internal class CurveLoopsMerger : ICurveLoopsMerger {
        /// <summary>
        /// Шаг увеличения оффсета контуров = 50 мм.
        /// </summary>
        private const double _offsetFeetStep = 0.164;
        /// <summary>
        /// Значение максимально допустимого оффсета = 1 м.
        /// </summary>
        private const double _offsetMax = 3.281;
        private readonly SolidOptions _solidOptions;


        public CurveLoopsMerger() {
            _solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
        }


        /// <summary>
        /// Объединяет замкнутые плоские контуры в один внешний контур.<br/>
        /// Если объединить не получилось, будет возвращен прямоугольник, описанный вокруг всех исходных контуров.<br/>
        /// Описание алгоритма смотреть в заголовке класса.
        /// </summary>
        public CurveLoop Merge(ICollection<CurveLoop> curveLoops) {
            double offset = 0;
            ICollection<CurveLoop> curveLoopsXOY = CreateCopyLoopsOnXOY(curveLoops);
            while(offset <= _offsetMax) {
                ICollection<CurveLoop> offsettedLoops = CreateOffsetLoop(curveLoopsXOY, offset);
                IList<Solid> solids = CreateSolids(offsettedLoops);
                if(TryMergeSolids(solids, out Solid mergedSolid)) {
                    Face bottomFace = GetBottomFace(mergedSolid);
                    CurveLoop bottomLoop = GetOuterCurveLoop(bottomFace);
                    Transform transform = GetVerticalTransformFromZero(curveLoops.First());
                    try {
                        return CurveLoop.CreateViaTransform(CreateOffsetLoop(bottomLoop, -offset), transform);
                    } catch(Exception ex) when(ex.GetType().Namespace.Contains(nameof(Autodesk))) {
                        offset += _offsetFeetStep;
                    }
                } else {
                    offset += _offsetFeetStep;
                }
            }
            return CreateRectangleLoop(curveLoops);
        }


        /// <summary>
        /// Возвращает трансформацию, которую нужно применить к точке (0; 0; 0), 
        /// чтобы поднять ее на координату Z первой точки CurveLoop.
        /// </summary>
        private Transform GetVerticalTransformFromZero(CurveLoop curveLoop) {
            double z = curveLoop.First().GetEndPoint(0).Z;
            return Transform.CreateTranslation(new XYZ(0, 0, z));
        }

        /// <summary>
        /// Создает копии контуров в плоскости XOY
        /// </summary>
        private ICollection<CurveLoop> CreateCopyLoopsOnXOY(ICollection<CurveLoop> curveLoops) {
            List<CurveLoop> copyLoops = new List<CurveLoop>();
            foreach(CurveLoop curveLoop in curveLoops) {
                var transform = GetVerticalTransformFromZero(curveLoop);
                copyLoops.Add(CurveLoop.CreateViaTransform(curveLoop, transform.Inverse));
            }
            return copyLoops;
        }

        /// <summary>
        /// Создает солид путем выдавливания вверх на 1 фут заданного замкнутого контура.
        /// </summary>
        private Solid CreateSolid(CurveLoop bottomLoop) {
            CurveLoop topLoop = CurveLoop.CreateViaTransform(bottomLoop, Transform.CreateTranslation(new XYZ(0, 0, 1)));
            return GeometryCreationUtilities.CreateLoftGeometry(new CurveLoop[] { bottomLoop, topLoop }, _solidOptions);
        }

        /// <summary>
        /// Пробует объединить список солидов в один солид.
        /// </summary>
        /// <param name="solids">Список солидов для объединения</param>
        /// <param name="mergedSolid">Объединенный солид</param>
        /// <returns>True, если удалось создать объединенный солид, иначе False</returns>
        private bool TryMergeSolids(IList<Solid> solids, out Solid mergedSolid) {
            mergedSolid = solids[0];
            for(int i = 1; i < solids.Count; i++) {
                try {
                    mergedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                        mergedSolid,
                        solids[i],
                        BooleanOperationsType.Union);
                } catch(Exception ex) when(ex.GetType().Namespace.Contains(nameof(Autodesk))) {
                    mergedSolid = default;
                    return false;
                }
            }
            return SolidUtils.SplitVolumes(mergedSolid).Count == 1;
        }

        /// <summary>
        /// Создает замкнутый контур с заданным оффсетом исходного контура в горизонтальной плоскости.<br/>
        /// Если оффсет не удалось создать по исходному контуру, будет создан оффсет по прямоугольнику, который описывает исходный контур.
        /// </summary>
        private CurveLoop CreateOffsetLoop(CurveLoop curveLoop, double feetOffset) {
            try {
                //если ориентация линий в контуре против часовой стрелки, то положительный оффсет увеличивает контур
                //если ориентация линий в контуре по часовой стрелке, то отрицательный оффсет увеличивает контур
                double offset = curveLoop.IsCounterclockwise(XYZ.BasisZ) ? feetOffset : -feetOffset;
                return CurveLoop.CreateViaOffset(curveLoop, offset, XYZ.BasisZ);
            } catch(Exception ex) when(ex.GetType().Namespace.Contains(nameof(Autodesk))) {
                return CurveLoop.CreateViaOffset(CreateRectangleLoop(curveLoop), feetOffset, XYZ.BasisZ);
            }
        }

        /// <summary>
        /// Возвращает коллекцию замкнутых контуров с заданным оффсетом исходных контуров в горизонтальной плоскости
        /// </summary>
        private ICollection<CurveLoop> CreateOffsetLoop(ICollection<CurveLoop> curveLoops, double feetOffset) {
            return curveLoops
                .Select(loop => CreateOffsetLoop(loop, feetOffset))
                .ToArray();
        }

        /// <summary>
        /// Возвращает список солидов, созданных из заданных замкнутых контуров
        /// </summary>
        private IList<Solid> CreateSolids(ICollection<CurveLoop> curveLoops) {
            return curveLoops
                .Select(loop => CreateSolid(loop))
                .ToArray();
        }

        /// <summary>
        /// Возвращает наружный контур грани солида
        /// </summary>
        private CurveLoop GetOuterCurveLoop(Face face) {
            return face.GetEdgesAsCurveLoops()
                .OrderByDescending(loop => loop.GetExactLength())
                .First();
        }

        /// <summary>
        /// Возвращает нижнюю грань солида
        /// </summary>
        private Face GetBottomFace(Solid solid) {
            var uv = new UV();
            var zVectorNegate = XYZ.BasisZ.Negate();
            return GetFaces(solid).First(face => face.ComputeNormal(uv).IsAlmostEqualTo(zVectorNegate));
        }

        private IEnumerable<Face> GetFaces(Solid solid) {
            foreach(Face face in solid.Faces) {
                yield return face;
            }
        }

        /// <summary>
        /// Создает прямоугольный замкнутый наружный контур, в который вписаны все заданные замкнутые контуры.<br/>
        /// Линии в этом контуре ориентированы против часовой стрелки.
        /// </summary>
        private CurveLoop CreateRectangleLoop(ICollection<CurveLoop> curveLoops) {
            var points = curveLoops
                .SelectMany(loop => loop.Select(curve => curve.GetEndPoint(0)))
                .ToArray();
            Outline outline = new Outline(points[0], points[1]);
            for(var i = 2; i < points.Length; i++) {
                outline.AddPoint(points[i]);
            }
            double z = outline.MinimumPoint.Z;
            double minX = outline.MinimumPoint.X;
            double minY = outline.MinimumPoint.Y;
            double maxX = outline.MaximumPoint.X;
            double maxY = outline.MaximumPoint.Y;

            XYZ leftBottom = new XYZ(minX, minY, z);
            XYZ leftTop = new XYZ(minX, maxY, z);
            XYZ rightTop = new XYZ(maxX, maxY, z);
            XYZ rightBottom = new XYZ(maxX, minY, z);

            var left = Line.CreateBound(leftTop, leftBottom);
            var bottom = Line.CreateBound(leftBottom, rightBottom);
            var right = Line.CreateBound(rightBottom, rightTop);
            var top = Line.CreateBound(rightTop, leftTop);

            return CurveLoop.Create(new Curve[] { left, bottom, right, top });
        }

        private CurveLoop CreateRectangleLoop(CurveLoop curveLoop) {
            return CreateRectangleLoop(new CurveLoop[] { curveLoop });
        }
    }
}
