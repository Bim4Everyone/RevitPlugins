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
    internal abstract class LevelProviderFactory : ILevelProviderFactory {
        private readonly Dictionary<ElementId, ILevelProvider> _providersCache =
            new Dictionary<ElementId, ILevelProvider>();
        
        protected readonly IResolutionRoot _resolutionRoot;
        protected readonly IElementPositionFactory _positionFactory;

        protected LevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory) {
            _resolutionRoot = resolutionRoot;
            _positionFactory = positionFactory;
        }

        public bool CanCreate(Element element) {
            if(element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            return _providersCache.ContainsKey(element.Category.Id) || CanCreateImpl(element);
        }

        public ILevelProvider Create(Element element) {
            if(element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            if(_providersCache.TryGetValue(element.Category.Id, out ILevelProvider levelProvider)) {
                return levelProvider;
            }

            levelProvider = CreateImpl(element);
            _providersCache.Add(element.Category.Id, levelProvider);

            return levelProvider;
        }

        protected virtual bool CanCreateImpl(Element element) {
            return element.InAnyCategory(GetLevelStairsProviderCategories())
                   || (_positionFactory.CanCreate(element)
                       && element.InAnyCategory(GetAllCategories()));
        }

        protected virtual ILevelProvider CreateImpl(Element element) {
            if(element.InAnyCategory(GetLevelNearestProviderCategories())) {
                return _resolutionRoot.Get<LevelNearestProvider>(GetConstructorArgument(element));
            } else if(element.InAnyCategory(GetLevelBottomProviderCategories())) {
                return _resolutionRoot.Get<LevelBottomProvider>(GetConstructorArgument(element));
            } else if(element.InAnyCategory(GetLevelMagicBottomProviderCategories())) {
                return _resolutionRoot.Get<LevelMagicBottomProvider>(GetConstructorArgument(element));
            } else if(element.InAnyCategory(GetLevelByIdProviderCategories())) {
                return _resolutionRoot.Get<LevelByIdProvider>(GetConstructorArgument(element));
            } else if(element.InAnyCategory(GetLevelStairsProviderCategories())) {
                return _resolutionRoot.Get<LevelStairsProvider>(new ConstructorArgument("factory", this));
            }

            throw new ArgumentException($"Переданный элемент \"{element.Id}\" с категорией \"{element.Category.Name}\" не поддерживается.");
        }

        protected ConstructorArgument GetConstructorArgument(Element element) {
            var elementPosition = _positionFactory.Create(element);
            return new ConstructorArgument("elementPosition", elementPosition);
        }

        private IEnumerable<BuiltInCategory> GetAllCategories() {
            return GetLevelNearestProviderCategories()
                .Union(GetLevelBottomProviderCategories())
                .Union(GetLevelMagicBottomProviderCategories())
                .Union(GetLevelByIdProviderCategories())
                .Union(GetLevelStairsProviderCategories());
        }
        
        private IEnumerable<BuiltInCategory> GetLevelNearestProviderCategories() {
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
            yield return BuiltInCategory.OST_StructuralColumns;
            yield return BuiltInCategory.OST_Stairs;
            yield return BuiltInCategory.OST_StructuralFoundation;
            yield return BuiltInCategory.OST_EdgeSlab;
            yield return BuiltInCategory.OST_PlumbingFixtures;
            yield return BuiltInCategory.OST_MechanicalEquipment;
        }

        private IEnumerable<BuiltInCategory> GetLevelBottomProviderCategories() {
            yield return BuiltInCategory.OST_Windows;
            yield return BuiltInCategory.OST_Ceilings;
            yield return BuiltInCategory.OST_RoofSoffit;
            yield return BuiltInCategory.OST_DuctCurves;
            yield return BuiltInCategory.OST_FlexDuctCurves;
            yield return BuiltInCategory.OST_PipeCurves;
            yield return BuiltInCategory.OST_FlexPipeCurves;
            yield return BuiltInCategory.OST_DuctAccessory;
            yield return BuiltInCategory.OST_PipeAccessory;
            yield return BuiltInCategory.OST_DuctLinings;
            yield return BuiltInCategory.OST_DuctInsulations;
            yield return BuiltInCategory.OST_PipeInsulations;
            yield return BuiltInCategory.OST_DuctFitting;
            yield return BuiltInCategory.OST_PipeFitting;
            yield return BuiltInCategory.OST_DuctTerminal;
            yield return BuiltInCategory.OST_Sprinklers;
            yield return BuiltInCategory.OST_LightingDevices;
            yield return BuiltInCategory.OST_DataDevices;
            yield return BuiltInCategory.OST_CableTray;
            yield return BuiltInCategory.OST_Conduit;
            yield return BuiltInCategory.OST_LightingFixtures;
            yield return BuiltInCategory.OST_CableTrayFitting;
            yield return BuiltInCategory.OST_ConduitFitting;
            yield return BuiltInCategory.OST_SecurityDevices;
            yield return BuiltInCategory.OST_FireAlarmDevices;
            yield return BuiltInCategory.OST_TelephoneDevices;
            yield return BuiltInCategory.OST_NurseCallDevices;
            yield return BuiltInCategory.OST_CommunicationDevices;
            yield return BuiltInCategory.OST_ElectricalEquipment;
            yield return BuiltInCategory.OST_ElectricalFixtures;
        }
        
        private IEnumerable<BuiltInCategory> GetLevelMagicBottomProviderCategories() {
            yield return BuiltInCategory.OST_StructuralFraming;
            yield return BuiltInCategory.OST_StructuralTruss;
        }
        
        private IEnumerable<BuiltInCategory> GetLevelByIdProviderCategories() {
            yield return BuiltInCategory.OST_Rooms;
            yield return BuiltInCategory.OST_Areas;
        }
        
        private IEnumerable<BuiltInCategory> GetLevelStairsProviderCategories() {
            yield return BuiltInCategory.OST_StairsRuns;
            yield return BuiltInCategory.OST_StairsLandings;
        }
    }
}