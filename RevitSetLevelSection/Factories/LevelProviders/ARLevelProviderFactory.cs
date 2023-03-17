using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.LevelProviders;

namespace RevitSetLevelSection.Factories.LevelProviders {
    internal class ARLevelProviderFactory : ILevelProviderFactory {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly IElementPositionFactory _positionFactory;

        public ARLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory) {
            _resolutionRoot = resolutionRoot;
            _positionFactory = positionFactory;
        }

        public ILevelProvider Create(Element element) {
            var elementPosition = _positionFactory.Create(element);
            var constructorArgument = new ConstructorArgument("elementPosition", elementPosition);
            
            if(element.InAnyCategory(GetLevelNearestProviderCategories())) {
                return _resolutionRoot.Get<LevelNearestProvider>(constructorArgument);
            } else if(element.InAnyCategory(BuiltInCategory.OST_Windows,
                          BuiltInCategory.OST_Ceilings,
                          BuiltInCategory.OST_RoofSoffit)) {
                return _resolutionRoot.Get<LevelBottomProvider>(constructorArgument);
            } else if(element.InAnyCategory(BuiltInCategory.OST_Rooms, 
                          BuiltInCategory.OST_Areas)) {
                return _resolutionRoot.Get<LevelByIdProvider>(constructorArgument);
            }

            return null;
        }

        private IEnumerable<BuiltInCategory> GetLevelNearestProviderCategories() {
            yield return BuiltInCategory.OST_Floors;
            yield return BuiltInCategory.OST_Windows;
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