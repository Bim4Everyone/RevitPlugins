using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models {
    public class RebarFinder {
        private readonly string _formNumberParamName = "обр_ФОП_Форма_номер";

        internal RebarFinder(MainViewModel mvm, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            SheetInfo = pylonSheetInfo;
        }

        internal MainViewModel ViewModel { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }

        public FamilyInstance GetSkeletonRebar(View view) {
            var rebars = new FilteredElementCollector(view.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach(Element rebar in rebars) {
                // Фильтрация по комплекту документации
                if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != SheetInfo.ProjectSection) {
                    continue;
                }
                // Фильтарция по имени семейства
                FamilySymbol rebarType = view.Document.GetElement(rebar.GetTypeId()) as FamilySymbol;
                if(rebarType is null) {
                    continue;
                }
                if(rebarType.FamilyName.Equals("IFC_Пилон_Верт.Арм.")) {
                    return rebar as FamilyInstance;
                }
            }
            return null;
        }

        public List<Element> GetSimpleRebars(View view, int formNumberMin, int formNumberMax) {
            var rebars = new FilteredElementCollector(view.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToElements();

            var simpleRebars = new List<Element>();
            foreach(Element rebar in rebars) {
                // Фильтрация по комплекту документации
                if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != SheetInfo.ProjectSection) {
                    continue;
                }
                // Фильтрация по номеру формы - отсев вертикальных стержней армирования
                var formNumber = rebar.GetParamValue<int>(_formNumberParamName);
                if(formNumber >= formNumberMin && formNumber <= formNumberMax) {
                    simpleRebars.Add(rebar);
                }
            }
            return simpleRebars;
        }
    }
}
