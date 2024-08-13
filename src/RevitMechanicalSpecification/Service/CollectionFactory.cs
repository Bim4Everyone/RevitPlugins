using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using dosymep.Revit;
using System.Windows.Forms;
using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Service {

    internal class CollectionFactory {

        private readonly Document _document;

        public CollectionFactory(Document doc) {
            _document = doc;
        }

        public List<Element> GetMechanicalElements() {
            var mechanicalCategories = new List<BuiltInCategory>()
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
                BuiltInCategory.OST_Sprinklers
            };
            return GetElements(mechanicalCategories);

        }

        public List<VisSystem> GetMechanicalSystemColl() {
            List<Element> elements = GetElements(
                new List<BuiltInCategory>() {
                BuiltInCategory.OST_PipingSystem,
                BuiltInCategory.OST_DuctSystem });

            var mechanicalSystems = new List<VisSystem>();
            mechanicalSystems.AddRange(elements.Select(element => new VisSystem {

                SystemElement = element as MEPSystem,
                SystemSystemName = element.Name,
                SystemFunction = element.GetElementType().GetSharedParamValueOrDefault<string>("ФОП_ВИС_ЭФ для системы"),
                SystemShortName = element.GetElementType().GetSharedParamValueOrDefault<string>("ФОП_ВИС_Сокращение для системы"),
                SystemTargetName = element.Name.Split(' ').First()
            }));

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

        private List<Element> GetElements(List<BuiltInCategory> builtInCategories) {
            var filter = new ElementMulticategoryFilter(builtInCategories);
            var elements = (List<Element>) new FilteredElementCollector(_document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();
            return elements.Where(e => LogicalFilter(e)).ToList();
        }

    }
}
