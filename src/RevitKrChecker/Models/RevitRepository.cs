using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitKrChecker.Models.Check;
using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models {
    internal class RevitRepository {

        private readonly List<BuiltInCategory> _categoriesForWork = new List<BuiltInCategory>() {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Stairs,
            BuiltInCategory.OST_StructConnections,
            BuiltInCategory.OST_Roofs
        };


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Element> GetViewElements() {
            var filter = new ElementMulticategoryFilter(_categoriesForWork);
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        public List<Element> GetPjElements() {
            var filter = new ElementMulticategoryFilter(_categoriesForWork);
            return new FilteredElementCollector(Document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        public List<ICheck> StoppingChecks() {
            List<ICheck> stoppingChecks = new List<ICheck>();

            //Параметр "ФОП_Блок СМР" должен содержать "Корпус", "Автостоянка", "Пристройка"
            List<string> trueValues = new List<string>() {
                "Корпус",
                "Автостоянка",
                "Пристройка"
            };
            var blockSmrCheck = new ParamCheck("ФОП_Блок СМР", "ФОП_Блок СМР", new ContainsCheckRule(), trueValues);

            stoppingChecks.Add(blockSmrCheck);
            return stoppingChecks;
        }

        public List<ICheck> NonStoppingChecks() {
            List<ICheck> nonStoppingChecks = new List<ICheck>();

            //Параметр "Секция СМР" должен быть заполнен
            var FOP_SekciaSMR_Check = new HasValueCheck("ФОП_Секция СМР", "ФОП_Секция СМР");

            nonStoppingChecks.Add(FOP_SekciaSMR_Check);
            return nonStoppingChecks;
        }
    }
}
