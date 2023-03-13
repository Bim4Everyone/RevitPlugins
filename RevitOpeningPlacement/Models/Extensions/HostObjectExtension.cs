using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using DevExpress.CodeParser;

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
                throw new ArgumentException($"{nameof(loops)} contains {loops.Count} loops, expected 2.");
            }
            return GeometryCreationUtilities.CreateBlendGeometry(
                loops[0],
                loops[1],
                null);
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
        /// если хост - "плитный" элемент, или содержащий переднюю и заднюю поверхности, если хост - стена.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если хост элемента не Стена | Потолок | Перекрытие | Крыша</exception>
        private static IList<Face> GetHostBoundFaces(this HostObject hostObject) {
            if(hostObject is Wall hostWall) {
                return hostWall.GetFaces().ToArray();
            } else if(hostObject is Floor hostFloor) {
                return new List<Face> {
                    hostFloor.GetBottomFace(),
                    hostFloor.GetTopFace()
                };
            } else if(hostObject is Ceiling hostCeiling) {
                return new List<Face> {
                    hostCeiling.GetBottomFace(),
                    hostCeiling.GetTopFace()
                };
            } else if(hostObject is RoofBase hostRoofBase) {
                return new List<Face> {
                    hostRoofBase.GetBottomFace(),
                    hostRoofBase.GetTopFace()
                };
            } else {
                throw new ArgumentException($"{nameof(hostObject)} is {hostObject.GetType().Name}, expected: Wall | Floor | Ceiling | RoofBase ");
            }
        }
        #endregion
    }
}
