using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class ParameterFilterInitializer {
        public static ParameterFilterElement GetOpeningFilter(Document doc) {
            var category = Category.GetCategory(doc, BuiltInCategory.OST_GenericModel);
            var nameParameter = ParameterFilterUtilities.GetFilterableParametersInCommon(doc, new[] { category.Id })
                .FirstOrDefault(item => item.IsSystemId() && (BuiltInParameter) item.IntegerValue == BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            FilterRule filterRule = default;
            if(nameParameter != null) {
                filterRule = ParameterFilterRuleFactory.CreateBeginsWithRule(nameParameter, "ОбщМд_Отв", false);
            }
            if(filterRule == null) {
                throw new ArgumentException("Отсутствует параметр \"Имя семейства\".", nameof(nameParameter));
            }
            return CreateFilter(doc, "BIM_Отверстия", new[] { BuiltInCategory.OST_GenericModel }, new[] { filterRule });
        }

        public static ParameterFilterElement GetMepFilter(Document doc) {
            return CreateFilter(doc,
                "BIM_Инж_Системы",
                new[] { BuiltInCategory.OST_DuctCurves,
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_Conduit,
                    BuiltInCategory.OST_CableTrayFitting,
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_ConduitFitting
                },
                new FilterRule[] { });
        }

        private static ParameterFilterElement CreateFilter(Document doc, string name, ICollection<BuiltInCategory> categories, ICollection<FilterRule> filterRules) {
            ParameterFilterElement filter = new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterFilterElement))
                .OfType<ParameterFilterElement>()
                .FirstOrDefault(item => item.Name.Equals(name));
            if(filter == null) {
                using(Transaction t = doc.StartTransaction("Создание фильтра")) {
                    filter = ParameterFilterElement.Create(doc, name, categories.Select(item => new ElementId(item)).ToArray());
                    if(filterRules.Any()) {
                        var logicalAndFilter = new LogicalAndFilter(filterRules.Select(item => new ElementParameterFilter(item)).ToArray());
                        filter.SetElementFilter(logicalAndFilter);
                    }

                    t.Commit();
                }
            }

            return filter;
        }
    }
}
