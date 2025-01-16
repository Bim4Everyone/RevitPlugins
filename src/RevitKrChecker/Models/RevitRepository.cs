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

            // Параметр "ФОП_Блок СМР" должен содержать "Корпус", "Автостоянка", "Пристройка"
            stoppingChecks.Add(new ParamCheck(
                "ФОП_Блок СМР",
                "ФОП_Блок СМР",
                LevelToFind.Instance,
                new ContainsCheckRule(),
                new List<string>() { "Корпус", "Автостоянка", "Пристройка" }));

            return stoppingChecks;
        }

        public List<ICheck> NonStoppingChecks() {
            List<ICheck> nonStoppingChecks = new List<ICheck>();

            // Параметр "Секция СМР" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck("ФОП_Секция СМР", "ФОП_Секция СМР", LevelToFind.Instance));
            // Параметр "Этаж СМР" должен быть заполнен
            nonStoppingChecks.Add(new HasValueCheck("ФОП_Этаж СМР", "ФОП_Этаж СМР", LevelToFind.Instance));
            // Параметр "Материал: Имя" начинается с: г02.02, г02.03, г02.04
            nonStoppingChecks.Add(new ParamCheck(
                "Материал: Имя",
                "Имя",
                LevelToFind.Material,
                new StartWithCheckRule(),
                new List<string>() { "г02.02", "г02.03", "г02.04" }));

            return nonStoppingChecks;
        }
    }
}
