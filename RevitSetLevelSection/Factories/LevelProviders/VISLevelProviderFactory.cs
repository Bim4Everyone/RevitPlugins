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
    internal class VISLevelProviderFactory : LevelProviderFactory {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly IElementPositionFactory _positionFactory;

        public VISLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory) {
            _resolutionRoot = resolutionRoot;
            _positionFactory = positionFactory;
        }
        
        protected override bool CanCreateImpl(Element element) {
            return _positionFactory.CanCreate(element)
                   && element.InAnyCategory(GetAllCategories());
        }

        protected override ILevelProvider CreateImpl(Element element) {
            var elementPosition = _positionFactory.Create(element);
            var constructorArgument = new ConstructorArgument("elementPosition", elementPosition);

            if(element.InAnyCategory(GetLevelNearestProviderCategories())) {
                return _resolutionRoot.Get<LevelNearestProvider>(constructorArgument);
            } else if(element.InAnyCategory(GetLevelBottomProviderCategories())) {
                return _resolutionRoot.Get<LevelBottomProvider>(constructorArgument);
            }

            throw new ArgumentException($"Переданный элемент \"{element.Id}\" с категорией \"{element.Category.Name}\" не поддерживается.");
        }
        
        private IEnumerable<BuiltInCategory> GetAllCategories() {
            return GetLevelBottomProviderCategories()
                .Union(GetLevelNearestProviderCategories());
        }

        private IEnumerable<BuiltInCategory> GetLevelNearestProviderCategories() {
            yield return BuiltInCategory.OST_PlumbingFixtures;
            yield return BuiltInCategory.OST_MechanicalEquipment;
        }

        private IEnumerable<BuiltInCategory> GetLevelBottomProviderCategories() {
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
    }
}