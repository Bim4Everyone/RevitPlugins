using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RevitMechanicalSpecification.Models.Classes;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {

    internal class CollectionFactory {

        private readonly Document _document;

        public CollectionFactory(Document doc) 
            {
            _document = doc;
            }

        public List<Element> GetMechanicalElements() {

            List<BuiltInCategory> mechanicalCategories = new List<BuiltInCategory>()
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
            return GetElements(mechanicalCategories);
            
        }




        public List<MechanicalSystem> GetMechanicalSystemColl() 
        {
            List<Element> elements = new List<Element>();
            List<MechanicalSystem> mechanicalSystems = new List<MechanicalSystem>();

            elements.AddRange(GetElements(BuiltInCategory.OST_PipingSystem));
            elements.AddRange(GetElements(BuiltInCategory.OST_DuctSystem));
            elements.Select(element => {
                var elementType = element.GetElementType();
                return new MechanicalSystem {
                    SystemElement = element as MEPSystem,
                    SystemName = element.Name,
                    SystemFunction = elementType.GetSharedParamValueOrDefault<string>("ФОП_ВИС_ЭФ для системы"),
                    SystemShortName = elementType.GetSharedParamValueOrDefault<string>("ФОП_ВИС_Сокращение для системы")
                };
            });
            return mechanicalSystems;
        }


        private bool LogicalFilter(Element element) {

            if(element is ModelText) {
                return false;
            }

            if(element.GroupId.IsNull()) {
                return true;
            }
            return false;
        }

        private List<Element> GetElements(List<BuiltInCategory> builtInCategories) 
            {
            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(builtInCategories);
            List<Element> elements = (List<Element>) new FilteredElementCollector(_document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();
            return elements.Where(e => LogicalFilter(e)).ToList();
        }

        private List<Element> GetElements(BuiltInCategory category) {
            List<Element> defColl = (List<Element>) new FilteredElementCollector(_document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements();

            return defColl.Where(e => LogicalFilter(e)).ToList();
            ;
        }
    }
}
