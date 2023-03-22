using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Factories.ElementPositions
{
    internal class VISElementPositionFactory : IElementPositionFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public VISElementPositionFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }
        
        public bool CanCreate(Element element) {
            return element.InAnyCategory(GetAllCategories());
        }
        
        public IElementPosition Create(Element element) {
            if(element.InAnyCategory(GetElementBottomPositionCategories())) {
                return _resolutionRoot.Get<ElementBottomPosition>();
            } 
            
            throw new ArgumentException($"Переданный элемент \"{element.Id}\" с категорией \"{element.Category.Name}\" не поддерживается.");
        }

        private IEnumerable<BuiltInCategory> GetAllCategories() {
            return GetElementBottomPositionCategories();
        }

        private IEnumerable<BuiltInCategory> GetElementBottomPositionCategories() {
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
            yield return BuiltInCategory.OST_PlumbingFixtures;
            yield return BuiltInCategory.OST_MechanicalEquipment;
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