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

        public static Filter GetWallFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetArchitectureFilter(RevitRepository.StructureCategoryNames[StructureCategoryEnum.Wall],
                                 revitRepository,
                                 BuiltInCategory.OST_Walls);
        }

        public static Filter GetFloorFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetArchitectureFilter(RevitRepository.StructureCategoryNames[StructureCategoryEnum.Floor],
                                 revitRepository,
                                 BuiltInCategory.OST_Floors);
        }

        public static Filter GetPipeFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.Pipe],
                                 revitRepository,
                                 BuiltInCategory.OST_PipeCurves,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        public static Filter GetRoundDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.RoundDuct],
                                 revitRepository,
                                 BuiltInCategory.OST_DuctCurves,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        public static Filter GetConduitFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.Conduit],
                                 revitRepository,
                                 BuiltInCategory.OST_Conduit,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        public static Filter GetRectangleDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minHeight, double minWidth) {
            var heightParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CURVE_HEIGHT_PARAM, minHeight); 
            var widthParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CURVE_WIDTH_PARAM, minWidth);

            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.RectangleDuct],
                                 revitRepository,
                                 BuiltInCategory.OST_DuctCurves,
                                 new[] { heightParamValuePair, widthParamValuePair });
        }

        public static Filter GetTrayFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minHeight, double minWidth) {
            var heightParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM, minHeight);
            var widthParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM, minWidth);

            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.CableTray],
                                 revitRepository,
                                 BuiltInCategory.OST_CableTray,
                                 new[] { heightParamValuePair, widthParamValuePair });
        }

        public static Filter GetTrayFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetFittingFilter(RevitRepository.FittingCategoryNames[FittingCategoryEnum.CableTrayFitting], revitRepository, BuiltInCategory.OST_CableTrayFitting);
        }

        public static Filter GetPipeFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetFittingFilter(RevitRepository.FittingCategoryNames[FittingCategoryEnum.PipeFitting], revitRepository, BuiltInCategory.OST_PipeFitting);
        }

        public static Filter GetDuctFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetFittingFilter(RevitRepository.FittingCategoryNames[FittingCategoryEnum.DuctFitting], revitRepository, BuiltInCategory.OST_DuctFitting);
        }

        public static Filter GetConduitFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetFittingFilter(RevitRepository.FittingCategoryNames[FittingCategoryEnum.ConduitFitting], revitRepository, BuiltInCategory.OST_ConduitFitting);
        }

        private static Filter GetdMepFilter(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category, IEnumerable<ParamValuePair> paramValuePairs) {
            return new Filter(revitRepository) {
                CategoryIds = new List<int> { (int) category },
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = GetCriterions(revitRepository, paramValuePairs).ToList(),
                    RevitRepository = revitRepository
                },
                RevitRepository = revitRepository
            };
        }

        private static Filter GetFittingFilter(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category) {
            return new Filter(revitRepository) {
                CategoryIds = new List<int> { (int) category },
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = new List<Criterion>(),
                    RevitRepository = revitRepository
                },
            };
        }

        public static Filter GetArchitectureFilter(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category) {
            return new Filter(revitRepository) {
                CategoryIds = new List<int> { (int) category },
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = new List<Criterion>(),
                    RevitRepository = revitRepository
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
                    Value = new DoubleParamValue(resultValue, paramValuePair.Value.ToString()),
                    RevitRepository = revitRepository
                };
            }
        }
    }

    internal class ParamValuePair {
        public RevitParam RevitParam { get; set; }
        public double Value { get; set; }

        public static ParamValuePair GetBuiltInParamValuePair(Document doc, BuiltInParameter param, double value) {
            return new ParamValuePair() {
                RevitParam = ParameterInitializer.InitializeParameter(doc, new ElementId(param)),
                Value = value
            };
        }
    }
}
