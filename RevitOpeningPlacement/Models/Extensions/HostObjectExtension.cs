using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using DevExpress.CodeParser;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    /// <summary>
    /// Методы расширения для классов - наследников <see cref="Autodesk.Revit.DB.HostObject"/>
    /// </summary>
    internal static class HostObjectExtension {
        #region Getting HostObject OriginalSolid
        /// <summary>
        /// Возвращает "оригинальный" Solid элемента, являющегося <see cref="Autodesk.Revit.DB.HostObject">основой</see> без вырезаний.
        /// Реализовано для элементов следующих типов: Wall | Floor | Ceiling | RoofBase
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если <see cref="GetHostBoundCurveLoops"/> возвращает список не из 2 петель кривых</exception>
        public static Solid GetHostElementOriginalSolid(this HostObject hostObject) {
            IList<CurveLoop> loops = GetHostBoundCurveLoops(hostObject);
            if(loops.Count != 2) {
                throw new ArgumentException($"{nameof(loops)} содержит {loops.Count} петли, ожидалось 2.");
            }
            SolidOptions opts = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            return GeometryCreationUtilities.CreateLoftGeometry(
                loops,
                opts);
        }

        /// <summary>
        /// Возвращает список, содержащий контуры линий 2-х граничных поверхностей геометрии <see cref="Autodesk.Revit.DB.HostObject">хоста</see>
        /// </summary>
        /// <returns></returns>
        private static IList<CurveLoop> GetHostBoundCurveLoops(this HostObject hostObject) {
            return GetHostBoundFaces(hostObject)
                .Select(face => face
                    .GetEdgesAsCurveLoops()
                    .OrderByDescending(loop => loop.GetExactLength())
                    .FirstOrDefault())
                .ToList();
        }

        /// <summary>
        /// Возвращает список, содержащий нижнюю и верхнюю поверхности <see cref="Autodesk.Revit.DB.HostObject">хоста</see>, 
        /// если хост - "плитный" элемент, или содержащий внутреннюю и внешнюю поверхности, если хост - стена.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="hostObject"/> не Стена | Потолок | Перекрытие | Крыша</exception>
        public static IEnumerable<Face> GetHostBoundFaces(this HostObject hostObject) {
            if(IsVerticallyCompound(hostObject)) {
                return hostObject.GetSideFaces().ToArray();
            } else {
                return new List<Face> {
                    hostObject.GetBottomFace(),
                    hostObject.GetTopFace()
                };
            }
        }
        #endregion

        /// <summary>
        /// Определяет, является ли <paramref name="hostObject"/> стеной.
        /// </summary>
        /// <param name="hostObject"></param>
        /// <returns>True, если <paramref name="hostObject"/> - стена, иначе False (перекрытия, крыши, потолки)</returns>
        public static bool IsVerticallyCompound(this HostObject hostObject) {
            return hostObject.GetElementType<HostObjAttributes>().GetCompoundStructure().IsVerticallyCompound;
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
    }
}
