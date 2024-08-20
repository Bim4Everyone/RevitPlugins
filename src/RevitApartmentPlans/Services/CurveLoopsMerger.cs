using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

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
        private readonly ICurveLoopsOffsetter _curveLoopsOffsetter;
        private readonly IRectangleLoopProvider _rectangleLoopProvider;

        public CurveLoopsMerger(
            ICurveLoopsOffsetter curveLoopsOffsetter,
            IRectangleLoopProvider rectangleLoopProvider) {

            _solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            _curveLoopsOffsetter = curveLoopsOffsetter
                ?? throw new ArgumentNullException(nameof(curveLoopsOffsetter));
            _rectangleLoopProvider = rectangleLoopProvider
                ?? throw new ArgumentNullException(nameof(rectangleLoopProvider));
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
                var unitedSolids = SolidExtensions.CreateUnitedSolids(solids)
                    .SelectMany(s => SolidUtils.SplitVolumes(s))
                    .ToArray();
                if(unitedSolids.Length == 1) {
                    var mergedSolid = unitedSolids.First();
                    Face bottomFace = GetBottomFace(mergedSolid);
                    CurveLoop bottomLoop = GetOuterCurveLoop(bottomFace);
                    Transform transform = GetVerticalTransformFromZero(curveLoops.First());
                    try {
                        return CurveLoop.CreateViaTransform(_curveLoopsOffsetter.CreateOffsetLoop(bottomLoop, -offset), transform);
                    } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                        offset += _offsetFeetStep;
                    }
                } else {
                    offset += _offsetFeetStep;
                }
            }
            return _rectangleLoopProvider.CreateRectCounterClockwise(curveLoops);
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
            try {
                CurveLoop topLoop = CurveLoop.CreateViaTransform(bottomLoop, Transform.CreateTranslation(new XYZ(0, 0, 1)));
                return GeometryCreationUtilities.CreateLoftGeometry(new CurveLoop[] { bottomLoop, topLoop }, _solidOptions);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                CurveLoop bottomRectangleLoop = _rectangleLoopProvider.CreateRectCounterClockwise(bottomLoop);
                CurveLoop topRectangleLoop = CurveLoop.CreateViaTransform(
                    bottomRectangleLoop, Transform.CreateTranslation(new XYZ(0, 0, 1)));
                return GeometryCreationUtilities.CreateLoftGeometry(
                    new CurveLoop[] { bottomRectangleLoop, topRectangleLoop }, _solidOptions);
            }
        }

        /// <summary>
        /// Возвращает коллекцию замкнутых контуров с заданным оффсетом исходных контуров в горизонтальной плоскости
        /// </summary>
        private ICollection<CurveLoop> CreateOffsetLoop(ICollection<CurveLoop> curveLoops, double feetOffset) {
            return curveLoops
                .Select(loop => _curveLoopsOffsetter.CreateOffsetLoop(loop, feetOffset))
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
            return solid.Faces.OfType<Face>();
        }
    }
}
