using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningSlopes.Models.Exceptions;

namespace RevitOpeningSlopes.Models {
    internal class SolidOperations {
        private readonly RevitRepository _revitRepository;
        private readonly PluginConfig _pluginConfig;
        private readonly ElementFilter _categoryFilter = new ElementMulticategoryFilter(
                new BuiltInCategory[] {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_Floors});
        private readonly ElementCategoryFilter _openingCategoryFilter =
            new ElementCategoryFilter(BuiltInCategory.OST_Windows);

        public SolidOperations(
            RevitRepository revitRepository,
            PluginConfig pluginConfig) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));
        }

        public Solid GetUnitedSolid(IEnumerable<Solid> solids) {
            return SolidExtensions.CreateUnitedSolids((IList<Solid>) solids)
                .OrderByDescending(s => s.Volume)
                .FirstOrDefault();
        }

        /// <summary>
        /// Возвращает объединенный Solid из твердотельной геометрии семейства категории "Окна"
        /// </summary>
        /// <param name="opening">Окно</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Срабатывает, если экземпляр окна равен null</exception>
        /// <exception cref="OpeningNullSolidException">Срабатывает, если внутри семейства окна не 
        /// обнаружена твердотельная геометрия (отверстия)</exception>
        public Solid GetUnitedSolidFromOpening(Element opening) {
            if(opening == null) {
                throw new ArgumentNullException(nameof(opening));
            }

            var dependingElements = opening.GetDependentElements(_openingCategoryFilter);
            IList<Solid> totalSolids = new List<Solid>();

            foreach(var depEl in dependingElements) {
                var openingSolid = GetUnitedSolid(_revitRepository.Document.GetElement(depEl).GetSolids());
                if(openingSolid.Volume > 0) {
                    totalSolids.Add(openingSolid);
                }
            }

            return totalSolids.Count > 0
                ? GetUnitedSolid(totalSolids)
                : throw new OpeningNullSolidException($"Отсутствует твердотельная геометрия в оконном проеме");
        }

        /// <summary>
        /// Функция создает объединенный Solid из элементов, находящихся внутри увеличенного BoundingBox семейства окна
        /// </summary>
        /// <returns>Объединенный Solid из Solid элементов вокруг окна</returns>
        public Solid GetUnitedSolidFromBoundingBox(Outline outlineWithOffset) {
            Solid nearestElementSolid = null;
            if(outlineWithOffset != null) {

                var bboxIntersectFilter =
                new BoundingBoxIntersectsFilter(outlineWithOffset);

                var nearestElements = new FilteredElementCollector(_revitRepository.Document)
                    .WhereElementIsNotElementType()
                    .WherePasses(_categoryFilter)
                    .WherePasses(bboxIntersectFilter)
                    .ToElements()
                    .Where(el => !IsExcludedWallType(el));

                IList<Solid> nearestSolids = nearestElements
                    .Select(el => GetUnitedSolid(el.GetSolids()))
                    .ToList();

                nearestElementSolid = GetUnitedSolid(nearestSolids);
            }
            return nearestElementSolid;
        }

        /// <summary>
        /// Проверяет принадлежность стены к исключенному типоразмеру
        /// </summary>
        /// <param name="element">Проверяемый элемент</param>
        /// <returns>True, если типоразмер стены исключен</returns>
        private bool IsExcludedWallType(Element element) {
            return element is Wall wall
                && _pluginConfig.ExcludedWallTypeIds != null
                && _pluginConfig.ExcludedWallTypeIds.Count != 0 && _pluginConfig.ExcludedWallTypeIds.Contains(wall.GetTypeId());
        }

        /// <summary>
        /// Вспомогательная функция для теста
        /// </summary>
        /// <param name="solid"></param>
        public void CreateDirectShape(Solid solid) {
            var ds = DirectShape.CreateElement(_revitRepository.Document,
                new ElementId(BuiltInCategory.OST_GenericModel));
            ds.SetShape(new GeometryObject[] { solid });
        }
    }
}
