using System;
using System.Collections.Generic;
using System.Web;

using dosymep.SimpleServices;

using RevitParamsChecker.ViewModels.Results;

namespace RevitParamsChecker.Services;

/// <summary>
/// Данные html отчета, которые нужны на всем протяжении записи
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

    public string TableHeader { get; }

    /// <summary>
    /// Якоря правил по их именам
    /// </summary>
    public IReadOnlyDictionary<string, string> RuleAnchors { get; }

    /// <summary>
    /// Создает html якорь правила по его индексу
    /// </summary>
    public string GetRuleAnchor(int index) {
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

    /// <summary>
    /// Создает словарь с именами правил и их html якорями на эти правила по id
    /// </summary>
    private IReadOnlyDictionary<string, string> GetRuleAnchors(CheckResultViewModel checkResult) {
        var anchors = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        int index = 0;
        foreach(var rule in checkResult.RulesStamp) {
            string name = rule.Name ?? string.Empty;
            if(!anchors.ContainsKey(name)) {
                anchors.Add(name, GetRuleAnchor(index));
            }

            index++;
        }

        return anchors;
    }
}
