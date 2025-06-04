using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models {
    public class RebarFinder {
        private readonly ParamValueService _paramValueService;
        private readonly string _formNumberParamName = "обр_ФОП_Форма_номер";

        internal RebarFinder(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            _paramValueService = new ParamValueService(repository);
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
                // Фильтрация по имени семейства
                FamilySymbol rebarType = view.Document.GetElement(rebar.GetTypeId()) as FamilySymbol;
                if(rebarType is null) {
                    continue;
                }
                if(rebarType.FamilyName.Equals("IFC_Пилон_Верт.Арм.") || rebarType.FamilyName.Contains("IFC_Каркас_Пилон")) {
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
                var formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
                if(formNumber >= formNumberMin && formNumber <= formNumberMax) {
                    simpleRebars.Add(rebar);
                }
            }
            return simpleRebars;
        }


        public List<Element> GetSimpleRebars(View view, int formNumberMin, int formNumberMax,
                                             int formNumberMinException, int formNumberMaxException) {
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
                var formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
                bool includeInNeededRange = formNumber >= formNumberMin && formNumber <= formNumberMax;
                bool includeInExceptionRange = formNumber >= formNumberMinException && formNumber <= formNumberMaxException;

                if(includeInNeededRange && !includeInExceptionRange) {
                    simpleRebars.Add(rebar);
                }
            }
            return simpleRebars;
        }



        public List<Element> GetSimpleRebars(View view, int neededFormNumber) {
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
                var formNumber = _paramValueService.GetParamValueAnywhere(rebar, _formNumberParamName);
                if(formNumber == neededFormNumber) {
                    simpleRebars.Add(rebar);
                }
            }
            return simpleRebars;
        }
    }
}
