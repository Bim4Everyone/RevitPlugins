using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    /// <summary>
    /// Методы расширения для классов - наследников <see cref="Autodesk.Revit.DB.HostObject"/>
    /// </summary>
    internal static class HostObjectExtension {
        /// <summary>
        /// Возвращает "оригинальный" Solid элемента, являющегося <see cref="Autodesk.Revit.DB.HostObject">основой</see> без вырезаний.
        /// Реализовано для элементов следующих типов: Wall | Floor | Ceiling | RoofBase
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Autodesk.Revit.Exceptions.InvalidOperationException"></exception>
        public static Solid GetHostElementOriginalSolid(this HostObject hostObject) {
            IList<IList<CurveLoop>> loops = GetHostBoundCurveLoops(hostObject);

            SolidOptions opts = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            Solid solid = default;
            foreach(IList<CurveLoop> loopsPair in loops) {
                Solid currentSolid = GeometryCreationUtilities.CreateLoftGeometry(loopsPair, opts);
                if(solid is null) {
                    solid = currentSolid;
                } else {
                    // сложение солидов, чтобы исключить пересечения
                    solid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, currentSolid, BooleanOperationsType.Union);
                }
            }
            return solid;
        }

        /// <summary>
        /// Возвращает верхнюю поверхность <paramref name="hostObject"/>, если он является "плитным" элементом: Перекрытие | Потолок | Крыша
        /// </summary>
        /// <param name="hostObject">Плитный элемент: Перекрытие | Потолок | Крыша</param>
        /// <returns>Верхнюю поверхность Перекрытия | Потолка | Крыши</returns>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="hostObject"/> не является Перекрытием | Потолком | Крышей</exception>
        public static Face GetTopFace(this HostObject hostObject) {
            if(!IsVerticallyCompound(hostObject)) {
                var faceRefs = HostObjectUtils.GetTopFaces(hostObject);
                return (Face) hostObject.GetGeometryObjectFromReference(faceRefs[0]);
            } else {
                throw new ArgumentException($"Слои структуры элемента с Id {hostObject.Id} расположены вертикально. Нельзя определить верхнюю поверхность.");
            }
        }

        /// <summary>
        /// Возвращает нижнюю поверхность <paramref name="hostObject"/>, если он является "плитным" элементом: Перекрытие | Потолок | Крыша
        /// </summary>
        /// <param name="hostObject">Плитный элемент: Перекрытие | Потолок | Крыша</param>
        /// <returns>Нижнюю поверхность Перекрытия | Потолка | Крыши</returns>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="hostObject"/> не является Перекрытием | Потолком | Крышей</exception>
        public static Face GetBottomFace(this HostObject hostObject) {
            if(!IsVerticallyCompound(hostObject)) {
                var faceRefs = HostObjectUtils.GetBottomFaces(hostObject);
                return (Face) hostObject.GetGeometryObjectFromReference(faceRefs[0]);
            } else {
                throw new ArgumentException($"Слои структуры элемента с Id {hostObject.Id} расположены вертикально. Нельзя определить нижнюю поверхность.");
            }
        }


        /// <summary>
        /// Возвращает перечисление, состоящее из внутренней и внешней поверхности <paramref name="hostObject"/>, если он является Стеной
        /// </summary>
        /// <param name="hostObject">Стена</param>
        /// <returns>перечисление, состоящее из внутренней и внешней поверхности Стены></returns>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="hostObject"/> не является Стеной</exception>
        public static IEnumerable<Face> GetSideFaces(this HostObject hostObject) {
            if(IsVerticallyCompound(hostObject)) {
                var interiorFace = HostObjectUtils.GetSideFaces(hostObject, ShellLayerType.Interior);
                var exteriorFace = HostObjectUtils.GetSideFaces(hostObject, ShellLayerType.Exterior);

                yield return (Face) hostObject.GetGeometryObjectFromReference(interiorFace[0]);
                yield return (Face) hostObject.GetGeometryObjectFromReference(exteriorFace[0]);
            } else {
                throw new ArgumentException($"Слои структуры элемента с Id {hostObject.Id} расположены не вертикально. Нельзя определить боковые поверхности.");
            }
        }


        /// <summary>
        /// Возвращает список, в каждом элементе которого содержатся по 2 CurveLoop для создания солида по ним
        /// </summary>
        /// <param name="hostObject"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static IList<IList<CurveLoop>> GetHostBoundCurveLoops(this HostObject hostObject) {
            Face[] faces = GetHostBoundFaces(hostObject);
            if(faces.Length != 2) { throw new InvalidOperationException(); }
            IList<CurveLoop> firstLoops = GetOuterCurveLoops(faces[0]);
            IList<CurveLoop> secondLoops = GetOuterCurveLoops(faces[1]);

            List<List<CurveLoop>> loops = new List<List<CurveLoop>>();
            if(firstLoops.Count == secondLoops.Count) {
                // если количество CurveLoop одинаковое, будем считать их расположенными друг против друга на гранях
                for(int i = 0; i < firstLoops.Count; i++) {
                    loops.Add(new List<CurveLoop>() {
                        SimplifyCurveLoop(firstLoops[i]),
                        SimplifyCurveLoop(secondLoops[i]),
                    });
                }
            } else {
                for(int firstLoopIndex = 0; firstLoopIndex < firstLoops.Count; firstLoopIndex++) {
                    for(int secondLoopIndex = 0; secondLoopIndex < secondLoops.Count; secondLoopIndex++) {
                        loops.Add(new List<CurveLoop>() {
                            SimplifyCurveLoop(firstLoops[firstLoopIndex]),
                            SimplifyCurveLoop(secondLoops[secondLoopIndex]),
                        });
                    }
                }
            }
            return loops.Cast<IList<CurveLoop>>().ToList();
        }

        /// <summary>
        /// Возвращает контур, в котором отрезки, идущие друг за другом и лежащие на одной прямой превращены в один
        /// </summary>
        /// <param name="curveLoopRaw"></param>
        /// <returns></returns>
        public static CurveLoop SimplifyCurveLoop(CurveLoop curveLoopRaw) {
            CurveLoop simplifiedCurves = new CurveLoop();
            List<Curve> curveLoopList = curveLoopRaw.ToList();

            Curve curvePrevious = null;
            for(int i = 0; i < curveLoopList.Count; i++) {
                Curve curveCurrent = curveLoopList[i];
                if(curvePrevious is null) {
                    curvePrevious = curveCurrent;
                    continue;
                }

                bool isCurveAdded = AppendCurve(ref curvePrevious, curveCurrent);
                if(i != (curveLoopList.Count - 1) && !isCurveAdded) {
                    simplifiedCurves.Append(curvePrevious);
                    curvePrevious = curveCurrent;
                    continue;
                }
                if(i == (curveLoopList.Count - 1)) {
                    if(isCurveAdded) {
                        simplifiedCurves.Append(curvePrevious);
                    } else {
                        simplifiedCurves.Append(curvePrevious);
                        simplifiedCurves.Append(curveCurrent);
                    }
                }
            }
            return simplifiedCurves;
        }

        /// <summary>
        /// Добавляет второй отрезок в конец первого отрезка, если отрезки лежат на одной прямой, 
        /// и конец первого - это начало второго
        /// </summary>
        /// <param name="curveStart">Первый отрезок</param>
        /// <param name="curveEnd">Второй отрезок</param>
        /// <returns></returns>
        private static bool AppendCurve(ref Curve curveStart, Curve curveEnd) {
            if(curveStart is null || curveEnd is null) {
                return false;
            }

            if((curveStart is Line lineStart) && (curveEnd is Line lineEnd)) {
                if(!lineStart.Direction.Normalize().IsAlmostEqualTo(lineEnd.Direction.Normalize())) {
                    return false;
                }

                if(curveStart.GetEndPoint(1).IsAlmostEqualTo(curveEnd.GetEndPoint(0))) {
                    curveStart = Line.CreateBound(curveStart.GetEndPoint(0), curveEnd.GetEndPoint(1));
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        /// <summary>
        /// Возвращает список CurveLoop из заданной грани, которые находятся снаружи друг друга
        /// </summary>
        /// <param name="face">Грань для получения CurveLoop</param>
        /// <returns></returns>
        private static IList<CurveLoop> GetOuterCurveLoops(Face face) {
            // Полученные CurveLoop из наружных граней хост элемента находятся либо внутри друг друга, либо снаружи.
            // Поэтому, чтобы определить, лежит ли CurveLoop внутри другой, достаточно проверить, лежит ли хотя бы одна точка первой CurveLoop внутри другой.
            // Для этого строим луч от точки из первой CurveLoop в любую сторону и считаем количество пересечений с ребрами из второй CurveLoop.
            // Если число пересечений нечетно, то точка внутри, если четно - снаружи.
            // Для построения солида хоста без вырезов нам нужны только CurveLoop с каждой из двух ограничивающих поверхностей, которые являются наибольшими и не вложенными друг в друга.
            // Для неплоских поверхностей хоста этот алгоритм работать не будет и для них будут добавлены все их CurveLoop. Будем считать эту неточность допустимой.
            IList<CurveLoop> outerLoops = new List<CurveLoop>();

            IList<CurveLoop> curveLoops = face.GetEdgesAsCurveLoops().OrderByDescending(loop => loop.GetExactLength()).ToList();
            for(int i = 0; i < curveLoops.Count; i++) {
                CurveLoop curveLoopCurrent = curveLoops[i];
                Curve curve = curveLoopCurrent.First();
                XYZ curveStart = curve.GetEndPoint(0);
                XYZ curveEnd = curve.GetEndPoint(1);
                XYZ curveDirection = (curveEnd - curveStart).Normalize();
                // вместо неограниченной прямой делаем отрезок длиной 120 метров, который будем принимать за луч
                Curve ray = Line.CreateBound(curveStart, curveStart + curveDirection * 400);

                bool curveLoopIsOuter = true;
                foreach(CurveLoop outerLoop in outerLoops) {
                    int intersectionsCount = outerLoop.Where(addedCurve => addedCurve.Intersect(ray) != SetComparisonResult.Disjoint).Count();
                    curveLoopIsOuter = curveLoopIsOuter && ((intersectionsCount % 2) == 0);
                    if(!curveLoopIsOuter) {
                        // текущая CurveLoop находится внутри одной из уже добавленных в список - нет смысла проверять дальше
                        break;
                    }
                }
                if(curveLoopIsOuter) {
                    // текущая CurveLoop находится снаружи всех уже добавленных - добавляем ее тоже
                    outerLoops.Add(SimplifyCurveLoop(curveLoopCurrent));
                }
            }
            return outerLoops;
        }

        /// <summary>
        /// Возвращает список, содержащий нижнюю и верхнюю поверхности <see cref="Autodesk.Revit.DB.HostObject">хоста</see>, 
        /// если хост - "плитный" элемент, или содержащий внутреннюю и внешнюю поверхности, если хост - стена.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="hostObject"/> не Стена | Потолок | Перекрытие | Крыша</exception>
        private static Face[] GetHostBoundFaces(this HostObject hostObject) {
            // TODO скорректировать алгоритм:
            // если чистовое отверстие размещено на наружной грани хоста так,
            // что часть этого отверстия находится вне хоста, то ограничивающие поверхности хоста будут с вырезами,
            // что в дальнейшем приведет к неправильному определению солида
            if(IsVerticallyCompound(hostObject)) {
                return hostObject.GetSideFaces().ToArray();
            } else {
                return new Face[] {
                    hostObject.GetBottomFace(),
                    hostObject.GetTopFace()
                };
            }
        }

        /// <summary>
        /// Определяет, является ли <paramref name="hostObject"/> стеной.
        /// </summary>
        /// <param name="hostObject"></param>
        /// <returns>True, если <paramref name="hostObject"/> - стена, иначе False (перекрытия, крыши, потолки)</returns>
        private static bool IsVerticallyCompound(this HostObject hostObject) {
            return hostObject.GetElementType<HostObjAttributes>().GetCompoundStructure()?.IsVerticallyCompound ?? false;
        }
    }
}
