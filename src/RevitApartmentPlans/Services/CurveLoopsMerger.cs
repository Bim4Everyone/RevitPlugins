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


        public CurveLoop Merge(ICollection<CurveLoop> curveLoops) {
            throw new NotImplementedException();
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
    }
}
