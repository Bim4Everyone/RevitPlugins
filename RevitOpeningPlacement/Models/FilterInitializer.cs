using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.FilterableValueProviders;
using Autodesk.Revit.DB;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Value;
using RevitClashDetective.Models.Utils;
using dosymep.Bim4Everyone;

namespace RevitOpeningPlacement.Models {
    internal class FiltersInitializer {
        public static Filter GetPipeFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.CategoryNames[MepCategoriesEnum.Pipe],
                                 revitRepository,
                                 BuiltInCategory.OST_PipeCurves,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        public static Filter GetRoundDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.CategoryNames[MepCategoriesEnum.RoundDuct],
                                 revitRepository,
                                 BuiltInCategory.OST_DuctCurves,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        public static Filter GetRectangleDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minHeight, double minWidth) {
            var heightParamValuePair = new ParamValuePair() {
                RevitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM)),
                Value = minHeight
            };

            var widthParamValuePair = new ParamValuePair() {
                RevitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)),
                Value = minWidth
            };

            return GetdMepFilter(RevitRepository.CategoryNames[MepCategoriesEnum.RectangleDuct],
                                 revitRepository,
                                 BuiltInCategory.OST_DuctCurves,
                                 new[] { heightParamValuePair, widthParamValuePair });
        }

        public static Filter GetTrayFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minHeight, double minWidth) {
            var heightParamValuePair = new ParamValuePair() {
                RevitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM)),
                Value = minHeight
            };

            var widthParamValuePair = new ParamValuePair() {
                RevitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM)),
                Value = minWidth
            };

            return GetdMepFilter(RevitRepository.CategoryNames[MepCategoriesEnum.Tray],
                                 revitRepository,
                                 BuiltInCategory.OST_CableTray,
                                 new[] { heightParamValuePair, widthParamValuePair });
        }

        private static Filter GetdMepFilter(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category, IEnumerable<ParamValuePair> paramValuePairs) {
            return new Filter(revitRepository) {
                CategoryIds = new List<int> { (int) category },
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = GetCriterions(revitRepository, paramValuePairs).ToList()
                },
                RevitRepository = revitRepository
            };
        }

        private static IEnumerable<Criterion> GetCriterions(RevitClashDetective.Models.RevitRepository revitRepository, IEnumerable<ParamValuePair> paramValuePairs) {
            foreach(var paramValuePair in paramValuePairs) {
                var value = DoubleValueParser.TryParse(paramValuePair.Value.ToString(), paramValuePair.RevitParam.UnitType, out double resultValue);
                yield return new Rule() {
                    Provider = new ParameterValueProvider(revitRepository, paramValuePair.RevitParam),
                    Evaluator = RuleEvaluatorUtils.GetRuleEvaluators(paramValuePair.RevitParam.StorageType).FirstOrDefault(item => item.Evaluator == RuleEvaluators.FilterNumericGreater),
                    Value = new DoubleParamValue(resultValue, paramValuePair.Value.ToString())
                };
            }
        }
    }

    internal class ParamValuePair {
        public RevitParam RevitParam { get; set; }
        public double Value { get; set; }
    }
}
