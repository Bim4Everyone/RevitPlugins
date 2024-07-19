using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {

    internal class MechanicalSystem
        {
        public MechanicalSystem() { }
        public string SystemName {  get; set; }
        public string SystemFunction {  get; set; }
        public string SystemShortName { get; set; }
        public MEPSystem SystemElement { get; set; }

        }

    internal class DefaultCollector {

        private readonly Document _document;

        public DefaultCollector(Document doc) 
            {
            _document = doc;
            }

        public List<Element> GetDefElementColls() {

            List<Element> defColls = new List<Element>();
            List<BuiltInCategory> allCategories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers,
                BuiltInCategory.OST_GenericModel
            };

            foreach(BuiltInCategory category in allCategories) {
                defColls.AddRange(MakeColl(category));
            }

            return defColls;
        }

        public List<MechanicalSystem> GetDefSystemColl() 
        {
            List<Element> elements = new List<Element>();
            List<MechanicalSystem> mechanicalSystems = new List<MechanicalSystem>();

            elements.AddRange(MakeColl(BuiltInCategory.OST_PipingSystem));
            elements.AddRange(MakeColl(BuiltInCategory.OST_DuctSystem));

            foreach(Element element in elements) 
            {
                MechanicalSystem mechanicalSystem = new MechanicalSystem {
                    SystemElement = element as MEPSystem,
                    SystemName = element.Name,
                    SystemFunction = _document.GetElement(element.GetTypeId()).GetParamValueOrDefault("ФОП_ВИС_ЭФ для системы", "Null").ToString(),
                    SystemShortName = _document.GetElement(element.GetTypeId()).GetParamValueOrDefault("ФОП_ВИС_Сокращение для системы", "Null").ToString()
                };

                mechanicalSystems.Add(mechanicalSystem);
            }

            return mechanicalSystems;
        }

        private bool LogicalFilter(Element element) {

            if(element.Category.IsId(BuiltInCategory.OST_GenericModel) && element.GetType().Name == "ModelText") {
                return false;
            }
            if(element.GroupId.ToString() == "-1") {
                return true;
            }
            return false;
        }

        
        private List<Element> MakeColl(BuiltInCategory category) {
            List<Element> defColl = (List<Element>) new FilteredElementCollector(_document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements();

            return defColl.Where(e => LogicalFilter(e)).ToList();
            ;
        }
    }
}
