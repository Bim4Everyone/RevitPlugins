using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    /// <summary>
    /// Класс для обертки списка экземпляров семейств заданий на отверстия
    /// </summary>
    internal class OpeningsGroup {
        /// <summary>
        /// Список объединенных солидов экземпляров семейств отверстий
        /// </summary>
        private List<Solid> _solids = new List<Solid>();
        public OpeningsGroup() { }

        public OpeningsGroup(ICollection<FamilyInstance> familyInstances) {
            Elements.AddRange(familyInstances);
        }
        public List<FamilyInstance> Elements { get; } = new List<FamilyInstance>();

        /// <summary>
        /// Возвращает список экземпляров семейств отверстий из поданного списка, которые пересекаются с <see cref="_solids">солидами</see> группы отверстий
        /// </summary>
        /// <param name="elements">Список экземпляров семейств, которые будут фильтроваться по пересечению</param>
        /// <returns>Экземпляры семейств из поданного списка, пересекающиеся с <see cref="_solids">солидами</see> группы отверстий</returns>
        public List<FamilyInstance> Intersects(IList<FamilyInstance> elements) {
            List<FamilyInstance> intersections = new List<FamilyInstance>();
            var doc = elements.FirstOrDefault()?.Document;
            if(doc != null) {
                foreach(var solid in _solids) {
                    intersections.AddRange(
                        new FilteredElementCollector(doc, elements.Select(item => item.Id).ToArray())
                        .WherePasses(new ElementIntersectsSolidFilter(solid))
                        .OfType<FamilyInstance>());
                }
            }
            return intersections;
        }

        /// <summary>
        /// Добавляет поданный экземпляр семейства в <see cref="Elements">список элементов</see>, 
        /// а также объединяет Solid поданного элемента с Solid из списка уже добавленнымх <see cref="_solids">солидов элементов</see>, если они пересекаются.
        /// </summary>
        /// <param name="element">Экземпляр семейства отверстия для добавления</param>
        public void AddElement(FamilyInstance element) {
            Elements.Add(element);
            Solid unitedSolid = null;
            for(int i = 0; i < _solids.Count; i++) {
                try {
                    unitedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(_solids[i], element.GetSolid(), BooleanOperationsType.Union);
                    if(unitedSolid.Volume > 0) {
                        _solids[i] = unitedSolid;
                        break;
                    }
                } catch {
                    continue;
                }
            }
            if(unitedSolid == null || unitedSolid.Volume <= 0) {
                _solids.Add(element.GetSolid());
            }
        }

        public void AddRangeElements(ICollection<FamilyInstance> familyInstances) {
            foreach(var fi in familyInstances) {
                AddElement(fi);
            }
        }
    }
}
