using System.Globalization;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Разделы 17–19: ФОП_ТИП_Армирование, ФОП_Номер АТС, ФОП_ТИП_Доп. сортировка.
/// </summary>
internal static class ReinforcementRules {
    /// <summary>
    /// Конструкции без армирования.
    /// </summary>
    private static readonly string[] _unreinforcedKeys = {
        "БПодготовка", "ЦПР", "ПСлой", "Засыпка", "Свая", "Изол", "Термовкладыш"
    };

    /// <summary>
    /// Параметры, из которых собирается ФОП_ТИП_Доп. сортировка (через «_»).
    /// </summary>
    private static readonly string[] _sortingParameters = {
        ModelCheckContext.BlockParameter,
        ModelCheckContext.SectionParameter,
        ModelCheckContext.FloorParameter,
        ModelCheckContext.MaterialTypeParameter,
        ModelCheckContext.SizeParameter,
        ModelCheckContext.ReinforcementParameter,
        ModelCheckContext.LocationParameter,
        ModelCheckContext.PurposeParameter
    };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        CheckReinforcement(context, findings);
        if(!context.IsPitModel) {
            CheckAtsNumber(context, findings);
        }

        CheckAdditionalSorting(context, findings);
    }

    private static void CheckReinforcement(ModelCheckContext context, FindingCollection findings) {
        List<ReinforcementRule> rules = context.Project.ReinforcementRules ?? new List<ReinforcementRule>();
        var problems = new GroupedElements();
        var unmatchedByBlock = new OrderedMap<string, List<ElementData>>();
        foreach(ElementData element in context.ConcreteElements) {
            string typeName = element.TypeName;
            bool isPrecastStair = (typeName.Contains("ЛМарш") || typeName.Contains("ЛПлощадка")) && typeName.Contains("Сб");
            if(ModelCheckContext.ContainsAny(typeName, _unreinforcedKeys) || isPrecastStair) {
                continue;
            }

            string value = element.GetParameterOrEmpty(ModelCheckContext.ReinforcementParameter).Trim();
            if(value.Length == 0) {
                problems.Add("ФОП_ТИП_Армирование не заполнен.", null, element);
                continue;
            }

            if(rules.Count == 0) {
                continue;
            }

            string block = element.GetParameterOrEmpty(ModelCheckContext.BlockParameter);
            ReinforcementRule rule = rules.FirstOrDefault(item => IsRuleMatch(item, element, block));
            if(rule == null) {
                string blockKey = block.Length > 0 ? block : "блок не заполнен";
                unmatchedByBlock.GetOrAdd(blockKey, () => new List<ElementData>()).Add(element);
                continue;
            }

            bool isNumber = double.TryParse(
                value.Replace(",", "."),
                NumberStyles.Float,
                PythonFormat.Invariant,
                out double reinforcement);
            bool isAllowed = isNumber
                             && (Math.Abs(reinforcement - rule.Value) < 0.01
                                 || (rule.AlternativeValues ?? new List<double>())
                                 .Select(PythonFormat.NumberText)
                                 .Contains(value));
            if(!isAllowed) {
                problems.Add(
                    "ФОП_ТИП_Армирование не соответствует нормативу ЗнП (табл. «Расход арматуры на 1 м³ бетона»).",
                    "«" + value + "» вместо " + PythonFormat.NumberText(rule.Value) + " кг/м³ — " + (rule.Note ?? ""),
                    element);
            }
        }

        problems.AddFindings(findings, ReportSections.Reinforcement, context.FileName);
        foreach(var unmatched in unmatchedByBlock) {
            findings.Add(
                ReportSections.Reinforcement,
                "ФОП_ТИП_Армирование не сверено: в таблице армирования ЗнП нет правила для этих конструкций — "
                + "проверить вручную.",
                unmatched.Value.Select(element => element.Id),
                context.FileName,
                FindingKinds.Clarify,
                "«" + unmatched.Key + "»: " + ModelCheckContext.DescribeTypes(unmatched.Value, 5));
        }
    }

    private static bool IsRuleMatch(ReinforcementRule rule, ElementData element, string block) {
        string typeName = element.TypeName;
        string purpose = element.GetParameterOrEmpty(ModelCheckContext.PurposeParameter);
        string cycle = typeName.Split(new[] { '_' }, 2)[0];
        if(rule.Blocks?.Count > 0 && !rule.Blocks.Any(block.StartsWith)) {
            return false;
        }

        if(rule.Cycles?.Count > 0 && !rule.Cycles.Contains(cycle)) {
            return false;
        }

        if(rule.TypeKeys?.Count > 0 && !rule.TypeKeys.Any(typeName.Contains)) {
            return false;
        }

        if(rule.Purposes != null && !rule.Purposes.Contains(purpose)) {
            return false;
        }

        return !(rule.Floors?.Count > 0)
               || rule.Floors.Contains(element.GetParameterOrEmpty(ModelCheckContext.FloorParameter));
    }

    private static void CheckAtsNumber(ModelCheckContext context, FindingCollection findings) {
        // «ФОП_Номер АТС» в платформе нет (SharedParamsConfig, проверено 30.10.2026) — имя строкой
        string modelAts = context.Model.ProjectInfo != null
                          && context.Model.ProjectInfo.TryGetValue("ФОП_Номер АТС", out string value)
            ? PythonFormat.Trim(value)
            : "";
        string projectAts = PythonFormat.Trim(context.Project.AtsNumber);
        if(modelAts.Length == 0) {
            findings.Add(
                ReportSections.AtsNumber,
                "Параметр «ФОП_Номер АТС» в Сведениях о проекте не заполнен (по ЗнП — АТС "
                + (projectAts.Length > 0 ? projectAts : "?") + ").",
                null,
                context.FileName);
            return;
        }

        if(projectAts.Length == 0) {
            return;
        }

        bool isSame = double.TryParse(modelAts.Replace(",", "."), NumberStyles.Float, PythonFormat.Invariant, out double model)
                      && double.TryParse(projectAts.Replace(",", "."), NumberStyles.Float, PythonFormat.Invariant, out double project)
            ? Math.Abs(model - project) < 1e-6
            : modelAts == projectAts;
        if(!isSame) {
            findings.Add(
                ReportSections.AtsNumber,
                "«ФОП_Номер АТС» не соответствует ЗнП (АТС " + projectAts + "). Исправить и перепроверить "
                + "ФОП_ТИП_Армирование по спецификации «Параметры_ФОП_ТИП_Армирование_АТС " + projectAts + "».",
                null,
                context.FileName,
                detail: "в модели «" + modelAts + "»");
        }
    }

    private static void CheckAdditionalSorting(ModelCheckContext context, FindingCollection findings) {
        var emptyElements = new List<ElementData>();
        var outdatedElements = new List<ElementData>();
        foreach(ElementData element in context.VorElements) {
            string? value = element.GetParameter(ModelCheckContext.AdditionalSortingParameter);
            if(value == null || value.Trim().Length == 0) {
                emptyElements.Add(element);
                continue;
            }

            string expected = string.Join("_", _sortingParameters.Select(element.GetParameterOrEmpty));
            if(value != expected) {
                outdatedElements.Add(element);
            }
        }

        if(emptyElements.Count > 0) {
            findings.Add(
                ReportSections.AdditionalSorting,
                "ФОП_ТИП_Доп. сортировка не заполнен.",
                emptyElements.Select(element => element.Id),
                context.FileName,
                detail: ModelCheckContext.DescribeTypes(emptyElements, 3));
        }

        if(outdatedElements.Count > 0) {
            findings.Add(
                ReportSections.AdditionalSorting,
                "ФОП_ТИП_Доп. сортировка не соответствует текущим значениям параметров (маска {Блок}_{Секция}_{Этаж}_"
                + "{Тип материала}_{Размер}_{Армирование}_{Местоположение}_{Назначение}) — перезаполнить плагином "
                + "«Перенос параметров» после исправления остальных параметров.",
                outdatedElements.Select(element => element.Id),
                context.FileName,
                detail: ModelCheckContext.DescribeTypes(outdatedElements, 3));
        }
    }
}
