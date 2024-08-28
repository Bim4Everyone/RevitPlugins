using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class ParameterChecker {
        private readonly Document _document;
        private readonly SpecConfiguration _configuration;
        private readonly List<string> _report;

        public ParameterChecker(Document document, SpecConfiguration specConfiguration) {
            _report = new List<string>();
            _document = document;
            _configuration = specConfiguration;
        }

        private readonly List<BuiltInCategory> _defCatSet = new List<BuiltInCategory>() {
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


        private string CheckParameterCategories(string paraname, List<BuiltInCategory> designatedBuiltInCategories) {
            if(!_document.IsExistsSharedParam(paraname)) {
                return $"Параметр {paraname} не существует в проекте.";
            } else {
                (Definition Definition, Binding Binding) parameterSettings = _document.GetSharedParamBinding(paraname);
                Binding parameterBinding = parameterSettings.Binding;
                IEnumerable<Category> parameterCategories = parameterBinding.GetCategories();

                HashSet<BuiltInCategory> builtInCategories = new HashSet<BuiltInCategory>(
                    parameterCategories.Select(category => category.GetBuiltInCategory())
                );

                List<string> missingCategories = designatedBuiltInCategories
                    .Where(builtInCategory => !builtInCategories.Contains(builtInCategory))
                    .Select(builtInCategory => Category.GetCategory(_document, builtInCategory).Name)
                    .ToList();

                if(missingCategories.Any()) {
                    string result = $"Параметр {paraname} не назначен для категорий: ";
                    result += string.Join(", ", missingCategories);
                    return result;
                }
                return null;
            }
        }

        private void FillReportList(string paraname, List<BuiltInCategory> designatedBuiltInCategories) {
            string prepareReport = CheckParameterCategories(paraname, designatedBuiltInCategories);
            if(!string.IsNullOrEmpty(prepareReport)) {
                _report.Add(prepareReport);
            }
        }

        public void ExecuteParameterCheck() {
            FillReportList(_configuration.TargetNameGroup, _defCatSet); //Проверяем ФОП_ВИС_Группирование
            FillReportList(_configuration.TargetNameFunction, _defCatSet); //Проверяем ФОП_Экономическая функция
            FillReportList(_configuration.TargetNameSystem, _defCatSet); //Проверяем ФОП_Экономическая функция
            FillReportList(_configuration.TargetNameName, _defCatSet); //Проверяем ФОП_ВИС_Наименование комбинированное
            FillReportList(_configuration.TargetNameMark, _defCatSet); //Проверяем ФОП_ВИС_Марка
            FillReportList(_configuration.TargetNameCode, _defCatSet); //Проверяем ФОП_ВИС_Код изделия
            FillReportList(_configuration.TargetNameCreator, _defCatSet); //Проверяем ФОП_ВИС_Код изделия
            FillReportList(_configuration.TargetNameUnit, _defCatSet); //Проверяем ФОП_ВИС_Единица измерения
            FillReportList(_configuration.TargetNameNumber, _defCatSet); //Проверяем ФОП_ВИС_Число

            if(_report.Count > 0) {
                foreach(string report in _report) {
                    Console.WriteLine(report);
                }
            }
        }
    }
}
