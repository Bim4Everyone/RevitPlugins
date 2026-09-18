using System;
using System.Collections.Generic;
using System.Web;

using dosymep.SimpleServices;

using RevitParamsChecker.ViewModels.Results;

namespace RevitParamsChecker.Services;

/// <summary>
/// Данные html отчета, которые считаются один раз и нужны на всем протяжении записи
/// </summary>
internal class HtmlReportContext {
    public HtmlReportContext(CheckResultViewModel checkResult, ILocalizationService localization) {
        CheckResult = checkResult ?? throw new ArgumentNullException(nameof(checkResult));
        if(localization is null) {
            throw new ArgumentNullException(nameof(localization));
        }

        Title = localization.GetLocalizedString("HtmlReport.Title", checkResult.Name);
        GroupHeaderFormat = localization.GetLocalizedString("HtmlReport.GroupHeader");
        GroupHeaderNoPropertyFormat = localization.GetLocalizedString("HtmlReport.GroupHeaderNoProperty");
        TableHeader = GetTableHeader(localization);
        RuleAnchors = GetRuleAnchors(checkResult);
    }

    public CheckResultViewModel CheckResult { get; }

    public string Title { get; }

    public string GroupHeaderFormat { get; }

    public string GroupHeaderNoPropertyFormat { get; }

    /// <summary>
    /// Готовая строка заголовка таблицы: она одинакова для всех групп
    /// </summary>
    public string TableHeader { get; }

    /// <summary>
    /// Якоря правил по их именам
    /// </summary>
    public IReadOnlyDictionary<string, string> RuleAnchors { get; }

    public string GetRuleAnchor(int index) {
        // якорь по индексу, а не по имени: имена правил могут повторяться
        // и содержать символы, недопустимые в идентификаторе html
        return $"rule-{index}";
    }

    private string GetTableHeader(ILocalizationService localization) {
        return "<tr><th></th>"
               + GetHeaderCell(localization, "HtmlReport.IdHeader")
               + GetHeaderCell(localization, "ResultsPage.CategoryNameHeader")
               + GetHeaderCell(localization, "ResultsPage.FamilyTypeNameHeader")
               + GetHeaderCell(localization, "ResultsPage.StatusHeader")
               + GetHeaderCell(localization, "ResultsPage.FileNameHeader")
               + GetHeaderCell(localization, "ResultsPage.RuleNameHeader")
               + GetHeaderCell(localization, "ResultsPage.DescriptionHeader")
               + "</tr>";
    }

    private string GetHeaderCell(ILocalizationService localization, string localizationKey) {
        return "<th>" + HttpUtility.HtmlEncode(localization.GetLocalizedString(localizationKey)) + "</th>";
    }

    private IReadOnlyDictionary<string, string> GetRuleAnchors(CheckResultViewModel checkResult) {
        var anchors = new Dictionary<string, string>(StringComparer.Ordinal);
        int index = 0;
        foreach(var rule in checkResult.RulesStamp) {
            string name = rule.Name ?? string.Empty;
            // при дубликатах имен выигрывает первое правило, как и в SelectedElementResult
            if(!anchors.ContainsKey(name)) {
                anchors.Add(name, GetRuleAnchor(index));
            }

            index++;
        }

        return anchors;
    }
}
