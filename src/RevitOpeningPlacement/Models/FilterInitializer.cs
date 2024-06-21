using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Utils;
using RevitClashDetective.Models.Value;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitOpeningPlacement.Models {
    internal class FiltersInitializer {

        /// <summary>
        /// Возвращает стандартный фильтр по категории "Стены"
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetWallFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetArchitectureFilter(RevitRepository.StructureCategoryNames[StructureCategoryEnum.Wall],
                                 revitRepository,
                                 BuiltInCategory.OST_Walls);
        }

        /// <summary>
        /// Возвращает стандартный фильтр по категории "Перекрытия"
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetFloorFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return GetArchitectureFilter(RevitRepository.StructureCategoryNames[StructureCategoryEnum.Floor],
                                 revitRepository,
                                 BuiltInCategory.OST_Floors);
        }

        /// <summary>
        /// Возвращает фильтр по категории "Трубы" с критерием "больше" заданного диаметра
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="minDiameter">Минимальный диаметр. Трубы с бОльшим диаметром будут проходить фильтр</param>
        /// <returns></returns>
        public static Filter GetPipeFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.Pipe],
                                 revitRepository,
                                 RevitRepository.MepPipeLinearCategory,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        /// <summary>
        /// Возвращает фильтр по категории "Воздуховоды", семейство "Воздуховоды круглого сечения", с критерием фильтрации "больше" заданного диаметра
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="minDiameter">Минимальный диаметр. Воздуховоды с бОльшим диаметром будут проходить фильтр</param>
        /// <returns></returns>
        public static Filter GetRoundDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.RoundDuct],
                                 revitRepository,
                                 RevitRepository.MepDuctLinearCategory,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        /// <summary>
        /// Возвращает фильтр по категории "Короба" с критерием фильтрации "больше" заданного диаметра
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="minDiameter">Минимальный диаметр. Короба с бОльшим диаметром будут проходить фильтр</param>
        /// <returns></returns>
        public static Filter GetConduitFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minDiameter) {
            var revitParam = ParameterInitializer.InitializeParameter(revitRepository.Doc, new ElementId(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM));
            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.Conduit],
                                 revitRepository,
                                 RevitRepository.MepConduitLinearCategory,
                                 new[] { new ParamValuePair { RevitParam = revitParam, Value = minDiameter } });
        }

        /// <summary>
        /// Возвращает фильтр по категории "Воздуховоды", семейство "Воздуховоды прямоугольного сечения", с критерием фильтрации "больше" заданных высоты и ширины сечения
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="minHeight">Минимальная высота сечения. Воздуховоды с бОльшей высотой сечения будут проходить фильтр</param>
        /// <param name="minWidth">Минимальная ширина сечения. Воздуховоды с бОльшей шириной сечения будут проходить фильтр</param>
        /// <returns></returns>
        public static Filter GetRectangleDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minHeight, double minWidth) {
            var heightParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CURVE_HEIGHT_PARAM, minHeight);
            var widthParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CURVE_WIDTH_PARAM, minWidth);

            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.RectangleDuct],
                                 revitRepository,
                                 RevitRepository.MepDuctLinearCategory,
                                 new[] { heightParamValuePair, widthParamValuePair });
        }

        /// <summary>
        /// Возвращает фильтр по категории "Кабельные лотки" с критерием фильтрации "больше" заданных высоты и ширины сечения
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="minHeight">Минимальная высота сечения. Кабельные лотки с бОльшей высотой сечения будут проходить фильтр</param>
        /// <param name="minWidth">Минимальная ширина сечения. Кабельные лотки с бОльшей шириной сечения будут проходить фильтр</param>
        /// <returns></returns>
        public static Filter GetTrayFilter(RevitClashDetective.Models.RevitRepository revitRepository, double minHeight, double minWidth) {
            var heightParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM, minHeight);
            var widthParamValuePair = ParamValuePair.GetBuiltInParamValuePair(revitRepository.Doc, BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM, minWidth);

            return GetdMepFilter(RevitRepository.MepCategoryNames[MepCategoryEnum.CableTray],
                                 revitRepository,
                                 RevitRepository.MepCableTrayLinearCategory,
                                 new[] { heightParamValuePair, widthParamValuePair });
        }

        /// <summary>
        /// Возвращает стандартный фильтр по категории "Соединительные детали кабельных лотков"
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetTrayFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return CreateFilterByCategory(
                RevitRepository.FittingCategoryNames[FittingCategoryEnum.CableTrayFitting],
                revitRepository,
                RevitRepository.MepCableTrayFittingCategories.ToArray());
        }

        /// <summary>
        /// Возвращает стандартный фильтр по категории "Соединительные детали трубопроводов"
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetPipeFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return CreateFilterByCategory(
                RevitRepository.FittingCategoryNames[FittingCategoryEnum.PipeFitting],
                revitRepository,
                RevitRepository.MepPipeFittingCategories.ToArray());
        }

        /// <summary>
        /// Возвращает стандартный фильтр по категориям "Соединительные детали воздуховодов" и "Арматура воздуховодов"
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetDuctFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return CreateFilterByCategory(
                RevitRepository.FittingCategoryNames[FittingCategoryEnum.DuctFitting],
                revitRepository,
                RevitRepository.MepDuctFittingCategories.ToArray());
        }

        /// <summary>
        /// Возвращает стандартный фильтр по категории "Соединительные детали коробов"
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetConduitFittingFilter(RevitClashDetective.Models.RevitRepository revitRepository) {
            return CreateFilterByCategory(
                RevitRepository.FittingCategoryNames[FittingCategoryEnum.ConduitFitting],
                revitRepository,
                RevitRepository.MepConduitFittingCategories.ToArray());
        }

        /// <summary>
        /// Возвращает стандартный фильтр по заданной категории строительных конструкций
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        public static Filter GetArchitectureFilter(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category) {
            return CreateFilterByCategory(name, revitRepository, category);
        }

        /// <summary>
        /// Возвращает фильтр по всем используемым категориям элементов инженерных систем
        /// </summary>
        /// <returns></returns>
        public static ElementMulticategoryFilter GetFilterByAllUsedMepCategories() {
            return new ElementMulticategoryFilter(GetAllUsedMepCategories());
        }

        /// <summary>
        /// Возвращает все используемые категории инженерных систем
        /// </summary>
        /// <returns></returns>
        public static ICollection<BuiltInCategory> GetAllUsedMepCategories() {
            List<BuiltInCategory> categories = new List<BuiltInCategory>();
            categories.AddRange(RevitRepository.MepPipeCategories);
            categories.AddRange(RevitRepository.MepDuctCategories);
            categories.AddRange(RevitRepository.MepCableTrayCategories);
            categories.AddRange(RevitRepository.MepConduitCategories);
            return categories;
        }

        /// <summary>
        /// Возвращает фильтр по всем используемым категориям конструкций
        /// </summary>
        /// <returns></returns>
        public static ElementMulticategoryFilter GetFilterByAllUsedStructureCategories() {
            return new ElementMulticategoryFilter(GetAllUsedStructureCategories());
        }

        /// <summary>
        /// Возвращает все используемые категории конструкций
        /// </summary>
        /// <returns></returns>
        public static ICollection<BuiltInCategory> GetAllUsedStructureCategories() {
            return new BuiltInCategory[] {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors
            };
        }

        /// <summary>
        /// Возвращает фильтр по всем используемым категориям чистовых проемов в АР
        /// /// </summary>
        /// <returns></returns>
        public static ElementFilter GetFilterByAllUsedOpeningsArCategories() {
            return new ElementMulticategoryFilter(GetAllUsedOpeningsCategories());
        }

        /// <summary>
        /// Возвращает фильтр по всем используемым категориям чистовых проемов в КР
        /// </summary>
        /// <returns></returns>
        public static ElementFilter GetFilterByAllUsedOpeningsKrCategories() {
            return new ElementCategoryFilter(BuiltInCategory.OST_GenericModel);
        }

        /// <summary>
        /// Возвращает все используемые категории проемов
        /// </summary>
        /// <returns></returns>
        public static ICollection<BuiltInCategory> GetAllUsedOpeningsCategories() {
            return new BuiltInCategory[] {
                BuiltInCategory.OST_Windows
            };
        }

        /// <summary>
        /// Создает фильтр по заданной категории элементов Revit с правилами фильтрации "больше или равно" заданных значений параметров
        /// </summary>
        /// <param name="name">Название фильтра</param>
        /// <param name="revitRepository">Репозиторий Revit, в котором происходит фильтрация</param>
        /// <param name="category">Категория элементов для фильтрации</param>
        /// <param name="paramValuePairs">Пары параметров и их значений, по которым формируются правила фильтрации "больше"</param>
        /// <returns></returns>
        private static Filter GetdMepFilter(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category, IEnumerable<ParamValuePair> paramValuePairs) {
            return new Filter(revitRepository) {
                CategoryIds = new List<ElementId> { new ElementId(category) },
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = GetCriterions(revitRepository, paramValuePairs).ToList(),
                    RevitRepository = revitRepository
                },
                RevitRepository = revitRepository
            };
        }

        /// <summary>
        /// Возвращает фильтр по заданной категории
        /// </summary>
        /// <param name="name">Название фильтра</param>
        /// <param name="revitRepository">Репозиторий Revit, в котором будет проходить фильтрация элементов</param>
        /// <param name="category">Категория элементов</param>
        /// <returns></returns>
        private static Filter CreateFilterByCategory(string name, RevitClashDetective.Models.RevitRepository revitRepository, BuiltInCategory category) {
            return new Filter(revitRepository) {
                CategoryIds = new List<ElementId> { new ElementId(category) },
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = new List<Criterion>(),
                    RevitRepository = revitRepository
                }
            };
        }

        /// <summary>
        /// Возвращает фильтр по заданной коллекции категорий
        /// </summary>
        /// <param name="name">Название фильтра</param>
        /// <param name="revitRepository">Репозиторий Revit, в котором будет проходить фильтрация элементов</param>
        /// <param name="categories">Коллекция категорий элементов</param>
        /// <returns></returns>
        private static Filter CreateFilterByCategory(string name, RevitClashDetective.Models.RevitRepository revitRepository, ICollection<BuiltInCategory> categories) {
            return new Filter(revitRepository) {
                CategoryIds = categories.Select(category => new ElementId(category)).ToList(),
                Name = name,
                Set = new Set() {
                    SetEvaluator = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And),
                    Criteria = new List<Criterion>(),
                    RevitRepository = revitRepository
                }
            };
        }

        /// <summary>
        /// Создает критерии фильтрации элементов по значениям заданных пар параметров и их значений. Правила критериев формируются как "больше или равно" полученного значения
        /// </summary>
        /// <param name="revitRepository">Репозиторий Revit, в котором происходит фильтрация</param>
        /// <param name="paramValuePairs">Пары параметров и их значений, по которым формируются правила фильтрации "больше"</param>
        /// <returns>Критерии фильтрации, сформированные по правилу "больше" полученных значений</returns>
        private static IEnumerable<Criterion> GetCriterions(RevitClashDetective.Models.RevitRepository revitRepository, IEnumerable<ParamValuePair> paramValuePairs) {
            foreach(var paramValuePair in paramValuePairs) {
                var value = DoubleValueParser.TryParse(paramValuePair.Value.ToString(), paramValuePair.RevitParam.UnitType, out double resultValue);
                yield return new Rule() {
                    Provider = new ParameterValueProvider(revitRepository, paramValuePair.RevitParam),
                    Evaluator = RuleEvaluatorUtils.GetRuleEvaluators(paramValuePair.RevitParam.StorageType).FirstOrDefault(item => item.Evaluator == RuleEvaluators.FilterNumericGreaterOrEqual),
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
