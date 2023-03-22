using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public bool CanCreate(Element element) {
            return _positionFactory.CanCreate(element)
                   && element.InAnyCategory(GetAllCategories());
        }

        public ILevelProvider Create(Element element) {
            var elementPosition = _positionFactory.Create(element);
            var constructorArgument = new ConstructorArgument("elementPosition", elementPosition);
            
            if(element.InAnyCategory(GetLevelNearestProviderCategories())) {
                return _resolutionRoot.Get<LevelNearestProvider>(constructorArgument);
            } else if(element.InAnyCategory(GetLevelBottomProviderCategories())) {
                return _resolutionRoot.Get<LevelBottomProvider>(constructorArgument);
            } else if(element.InAnyCategory(GetLevelByIdProviderCategories())) {
                return _resolutionRoot.Get<LevelByIdProvider>(constructorArgument);
            }

            throw new ArgumentException($"Переданный элемент \"{element.Id}\" с категорией \"{element.Category.Name}\" не поддерживается.");
        }
        
        private IEnumerable<BuiltInCategory> GetAllCategories() {
            return GetLevelNearestProviderCategories()
                .Union(GetLevelBottomProviderCategories())
                .Union(GetLevelByIdProviderCategories());
        }

        private IEnumerable<BuiltInCategory> GetLevelBottomProviderCategories() {
            yield return BuiltInCategory.OST_Windows;
            yield return BuiltInCategory.OST_Ceilings;
            yield return BuiltInCategory.OST_RoofSoffit;
        }
        
        private IEnumerable<BuiltInCategory> GetLevelByIdProviderCategories() {
            yield return BuiltInCategory.OST_Rooms;
            yield return BuiltInCategory.OST_Areas;
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