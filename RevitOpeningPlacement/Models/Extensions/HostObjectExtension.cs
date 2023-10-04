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
                Solid currentSolid = GeometryCreationUtilities.CreateLoftGeometry(
                loopsPair,
                opts);
                if(solid is null) {
                    solid = currentSolid;
                } else {
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
            IList<CurveLoop> firstLoops = faces[0].GetEdgesAsCurveLoops();
            IList<CurveLoop> secondLoops = faces[1].GetEdgesAsCurveLoops();
            if(firstLoops.Count != secondLoops.Count) { throw new InvalidOperationException(); }

            List<List<CurveLoop>> loops = new List<List<CurveLoop>>();
            for(int i = 0; i < firstLoops.Count; i++) {
                loops.Add(new List<CurveLoop>() {
                    firstLoops[i],
                    secondLoops[i],
                });
            }
            return loops.Cast<IList<CurveLoop>>().ToList();
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
            return hostObject.GetElementType<HostObjAttributes>().GetCompoundStructure().IsVerticallyCompound;
        }

        /// <summary>
        /// Возвращает нижнюю поверхность <paramref name="hostObject"/>, если он является "плитным" элементом: Перекрытие | Потолок | Крыша
        /// </summary>
        /// <param name="hostObject">Плитный элемент: Перекрытие | Потолок | Крыша</param>
        /// <returns>Нижнюю поверхность Перекрытия | Потолка | Крыши</returns>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="hostObject"/> не является Перекрытием | Потолком | Крышей</exception>
        private static Face GetBottomFace(this HostObject hostObject) {
            if(!IsVerticallyCompound(hostObject)) {
                var faceRefs = HostObjectUtils.GetBottomFaces(hostObject);
                return (Face) hostObject.GetGeometryObjectFromReference(faceRefs[0]);
            } else {
                throw new ArgumentException($"Слои структуры элемента с Id {hostObject.Id} расположены вертикально. Нельзя определить нижнюю поверхность.");
            }
        }
    }
}
