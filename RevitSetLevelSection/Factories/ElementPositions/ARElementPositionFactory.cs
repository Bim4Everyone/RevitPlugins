using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Factories.ElementPositions {
    internal class ARElementPositionFactory : IElementPositionFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public ARElementPositionFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public IElementPosition Create(Element element) {
            if(element.InAnyCategory(BuiltInCategory.OST_Floors)) {
                return _resolutionRoot.Get<ElementTopPosition>();
            } else if(element.InAnyCategory(BuiltInCategory.OST_Windows)) {
                return _resolutionRoot.Get<ElementMiddlePosition>();
            } else if(element.InAnyCategory(GetElementBottomPositionCategories())) {
                return _resolutionRoot.Get<ElementBottomPosition>();
            }

            return null;
        }

        private IEnumerable<BuiltInCategory> GetElementBottomPositionCategories() {
            yield return BuiltInCategory.OST_Ceilings;
            yield return BuiltInCategory.OST_RoofSoffit;
            yield return BuiltInCategory.OST_Walls;
            yield return BuiltInCategory.OST_Doors;
            yield return BuiltInCategory.OST_GenericModel;
            yield return BuiltInCategory.OST_Roofs;
            yield return BuiltInCategory.OST_Stairs;
            yield return BuiltInCategory.OST_Columns;
            yield return BuiltInCategory.OST_Parts;
            yield return BuiltInCategory.OST_Gutter;
            yield return BuiltInCategory.OST_StairsRailing;
            yield return BuiltInCategory.OST_Parking;
            yield return BuiltInCategory.OST_Roads;
            yield return BuiltInCategory.OST_MechanicalEquipment;
            yield return BuiltInCategory.OST_Ramps;
            yield return BuiltInCategory.OST_Furniture;
            yield return BuiltInCategory.OST_FurnitureSystems;
            yield return BuiltInCategory.OST_Casework;
            yield return BuiltInCategory.OST_PlumbingFixtures;
            yield return BuiltInCategory.OST_SpecialityEquipment;
            yield return BuiltInCategory.OST_CurtaSystem;
        }
    }
}