using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows.Data;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitParamsChecker.Models.Results;
using RevitParamsChecker.ViewModels.Results;
using RevitParamsChecker.ViewModels.Rules;

namespace RevitParamsChecker.Services;

/// <summary>
/// Экспорт отчета по результату проверки в html файл.
/// Отчет повторяет то, что видно на странице анализа: текущие уровни группировки и текущий поиск.
/// </summary>
internal class HtmlReportExportService : IReportExportService {
    private const string _filter = "HTML files (*.html)|*.html";
    private const string _extension = ".html";
    private const string _defaultFileName = "report";

    private const string _style = @"
body{font-family:Segoe UI,Arial,sans-serif;font-size:13px;margin:16px;color:#1a1a1a}
h1{font-size:20px;margin:0 0 12px}
h2{font-size:16px;margin:24px 0 8px}
h3{font-size:14px;margin:0 0 4px}
details{margin:2px 0 2px 14px}
summary{cursor:pointer;padding:2px 0;font-weight:600}
table{border-collapse:collapse;margin:4px 0 10px 14px}
th,td{border:1px solid #c8c8c8;padding:2px 6px;text-align:left;vertical-align:top}
th{background:#f0f0f0;white-space:nowrap}
td.status{width:8px;padding:0;border-left:none;border-right:none}
.rule{border:1px solid #c8c8c8;border-radius:4px;padding:8px;margin:8px 0}
.set{border:1px solid #c8c8c8;border-left-width:4px;border-left-color:#008000;border-radius:4px;padding:6px;margin:4px 0 4px 8px}
.set.or{border-left-color:#0000ff}
.op{font-weight:600;margin-bottom:4px}
.param{margin:2px 0}
.param .name{font-weight:600}
";

    private readonly ILocalizationService _localization;
    private readonly ReportExportConfig _config;

    public HtmlReportExportService(ILocalizationService localization, ReportExportConfig config) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    public void Export(ISaveFileDialogService saveFileDialogService, CheckResultViewModel checkResult) {
        if(saveFileDialogService is null) {
            throw new ArgumentNullException(nameof(saveFileDialogService));
        }

        if(checkResult is null) {
            throw new ArgumentNullException(nameof(checkResult));
        }

        saveFileDialogService.Filter = _filter;
        saveFileDialogService.DefaultExt = _extension;
        if(!saveFileDialogService.ShowDialog(_config.DirPath, GetDefaultFileName(checkResult.Name))) {
            return;
        }

        WriteFile(saveFileDialogService.File.FullName, checkResult);
        _config.DirPath = saveFileDialogService.File.DirectoryName;
        _config.SaveProjectConfig();
    }

    private string GetDefaultFileName(string title) {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string name = new string((title ?? string.Empty)
            .Where(c => !invalidChars.Contains(c))
            .ToArray()).Trim();
        return string.IsNullOrWhiteSpace(name) ? _defaultFileName : name;
    }

    private void WriteFile(string path, CheckResultViewModel checkResult) {
        string tempPath = path + ".tmp";
        try {
            using(var writer = new StreamWriter(tempPath)) {
                Write(checkResult, writer);
            }

            if(File.Exists(path)) {
                File.Delete(path);
            }

            File.Move(tempPath, path);
        } finally {
            if(File.Exists(tempPath)) {
                File.Delete(tempPath);
            }
        }
    }

    private void Write(CheckResultViewModel checkResult, TextWriter writer) {
        var context = new HtmlReportContext(checkResult, _localization);
        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html>");
        writer.WriteLine("<head>");
        writer.WriteLine("<meta charset=\"utf-8\">");
        writer.Write("<title>");
        WriteEncoded(context.Title, writer);
        writer.WriteLine("</title>");
        writer.Write("<style>");
        writer.Write(_style);
        writer.WriteLine("</style>");
        writer.WriteLine("</head>");
        writer.WriteLine("<body>");
        writer.Write("<h1>");
        WriteEncoded(context.Title, writer);
        writer.WriteLine("</h1>");

        WriteSectionHeader("ResultsPage.ElementsHeader", writer);
        WriteElements(context, writer);
        WriteSectionHeader("ResultsPage.RulesHeader", writer);
        WriteRules(context, writer);

        writer.WriteLine("</body>");
        writer.WriteLine("</html>");
    }

    private void WriteSectionHeader(string localizationKey, TextWriter writer) {
        writer.Write("<h2>");
        WriteEncoded(_localization.GetLocalizedString(localizationKey), writer);
        writer.WriteLine("</h2>");
    }

    private void WriteElements(HtmlReportContext context, TextWriter writer) {
        var view = context.CheckResult.ElementResults.View;
        int visibleLevels = GetVisibleGroupLevels(context.CheckResult.ElementResults);
        if(visibleLevels <= 0 || view.Groups is null || view.Groups.Count == 0) {
            // группировки нет, либо остался только служебный уровень частей - плоская таблица
            WriteTable(view.OfType<ElementResultViewModel>(), context, writer);
            return;
        }

        foreach(var group in view.Groups.OfType<CollectionViewGroup>()) {
            WriteGroup(group, 0, visibleLevels, context, writer);
        }
    }

    /// <summary>
    /// Записывает группу элементов и все вложенные в нее группы
    /// </summary>
    /// <param name="group">Группа элементов</param>
    /// <param name="level">Индекс текущего уровня группировки, начинается с 0</param>
    /// <param name="visibleLevels">Общее количество уровней группировки, которые надо показать в отчете</param>
    /// <param name="context">Данные отчета</param>
    /// <param name="writer">Поток записи отчета</param>
    private void WriteGroup(
        CollectionViewGroup group,
        int level,
        int visibleLevels,
        HtmlReportContext context,
        TextWriter writer) {
        writer.Write("<details><summary>");
        writer.Write(GetGroupHeader(group, level, context));
        writer.WriteLine("</summary>");
        if(level >= visibleLevels - 1) {
            // самый глубокий видимый уровень: пишем все элементы поддерева одной таблицей,
            // тем самым схлопывая служебный уровень частей
            WriteTable(GetLeafElements(group), context, writer);
        } else {
            foreach(var innerGroup in group.Items.OfType<CollectionViewGroup>()) {
                WriteGroup(innerGroup, level + 1, visibleLevels, context, writer);
            }
        }

        writer.WriteLine("</details>");
    }

    /// <summary>
    /// Возвращает готовый заголовок группы для тега summary
    /// </summary>
    /// <param name="group">Группа элементов</param>
    /// <param name="level">Номер уровня группировки, нумерация начинается с 0</param>
    /// <param name="context">Данные отчета</param>
    private string GetGroupHeader(CollectionViewGroup group, int level, HtmlReportContext context) {
        string value = HttpUtility.HtmlEncode(group.Name?.ToString() ?? string.Empty);
        string propertyName = GetGroupPropertyName(context.CheckResult, level);
        return string.IsNullOrEmpty(propertyName)
            ? string.Format(context.GroupHeaderNoPropertyFormat, value, group.ItemCount)
            : string.Format(
                context.GroupHeaderFormat, HttpUtility.HtmlEncode(propertyName), value, group.ItemCount);
    }

    /// <summary>
    /// Возвращает название свойства, по которому сгруппированы элементы на заданном уровне,
    /// или null, если на этом уровне его определить нельзя
    /// </summary>
    /// <param name="checkResult">Результат проверки</param>
    /// <param name="level">Номер уровня группировки, нумерация начинается с 0</param>
    private string GetGroupPropertyName(CheckResultViewModel checkResult, int level) {
        var groupDescriptions = checkResult.GroupingProperties.GroupDescriptions;
        // пользователь мог поменять комбобоксы, не применив изменения,
        // тогда настройки группировки и само представление временно рассинхронизированы
        if(level < 0
           || level >= groupDescriptions.Count) {
            return null;
        }

        return groupDescriptions[level].SelectedProperty?.DisplayName;
    }

    /// <summary>
    /// Количество уровней группировки, которые надо показать в отчете:
    /// все настроенные пользователем, без служебного уровня в конце
    /// </summary>
    private int GetVisibleGroupLevels(CollectionViewSource elementResults) {
        var groupDescriptions = elementResults.GroupDescriptions;
        int levels = groupDescriptions.Count;
        // служебный уровень всегда ровно один и всегда последний
        if(levels > 0 && IsChunkLevel(groupDescriptions[levels - 1])) {
            levels--;
        }

        return levels;
    }

    /// <summary>
    /// Уровень группировки по ChunkName служебный: он нужен только чтобы
    /// экспандер в таблице не раскрывался долго на больших группах. В отчете он не нужен.
    /// </summary>
    private bool IsChunkLevel(GroupDescription groupDescription) {
        return groupDescription is PropertyGroupDescription propertyGroup
               && propertyGroup.PropertyName == nameof(ElementResultViewModel.ChunkName);
    }

    private IEnumerable<ElementResultViewModel> GetLeafElements(CollectionViewGroup group) {
        foreach(object item in group.Items) {
            if(item is CollectionViewGroup innerGroup) {
                foreach(var element in GetLeafElements(innerGroup)) {
                    yield return element;
                }
            } else if(item is ElementResultViewModel element) {
                yield return element;
            }
        }
    }

    private void WriteTable(
        IEnumerable<ElementResultViewModel> elements,
        HtmlReportContext context,
        TextWriter writer) {
        writer.WriteLine("<table>");
        writer.WriteLine(context.TableHeader);
        foreach(var element in elements) {
            WriteRow(element, context, writer);
        }

        writer.WriteLine("</table>");
    }

    private void WriteRow(ElementResultViewModel element, HtmlReportContext context, TextWriter writer) {
        writer.Write("<tr><td class=\"status\" style=\"background:");
        writer.Write(GetStatusColor(element.ElementResult.Status));
        writer.Write("\"></td><td>");
        writer.Write(element.Id.GetIdValue());
        writer.Write("</td><td>");
        WriteEncoded(element.CategoryName, writer);
        writer.Write("</td><td>");
        WriteEncoded(element.FamilyTypeName, writer);
        writer.Write("</td><td>");
        WriteEncoded(element.Status, writer);
        writer.Write("</td><td>");
        WriteEncoded(element.FileName, writer);
        writer.Write("</td><td>");
        WriteRuleLink(element, context, writer);
        writer.Write("</td><td>");
        WriteEncoded(element.Info, writer);
        writer.WriteLine("</td></tr>");
    }

    private void WriteRuleLink(ElementResultViewModel element, HtmlReportContext context, TextWriter writer) {
        string ruleName = element.RuleName ?? string.Empty;
        if(context.RuleAnchors.TryGetValue(ruleName, out string anchor)) {
            writer.Write("<a href=\"#");
            writer.Write(anchor);
            writer.Write("\">");
            WriteEncoded(ruleName, writer);
            writer.Write("</a>");
        } else {
            WriteEncoded(ruleName, writer);
        }
    }

    /// <summary>
    /// Цвета должны совпадать со StatusToColorConverter из CheckResultControl.xaml
    /// </summary>
    private string GetStatusColor(StatusCode status) {
        return status switch {
            StatusCode.Valid => "#008000",
            StatusCode.Invalid => "#FF0000",
            StatusCode.ParamNotFound => "#FFA500",
            StatusCode.Error => "#FF00FF",
            _ => "#808080"
        };
    }

    private void WriteRules(HtmlReportContext context, TextWriter writer) {
        int index = 0;
        foreach(var rule in context.CheckResult.RulesStamp) {
            writer.Write("<div class=\"rule\" id=\"");
            writer.Write(context.GetRuleAnchor(index));
            writer.WriteLine("\">");
            writer.Write("<h3>");
            WriteEncoded(rule.Name, writer);
            writer.WriteLine("</h3>");
            if(!string.IsNullOrWhiteSpace(rule.Description)) {
                writer.Write("<p>");
                WriteEncoded(rule.Description, writer);
                writer.WriteLine("</p>");
            }

            WriteParamsSet(rule.RootSet, writer);
            writer.WriteLine("</div>");
            index++;
        }
    }

    private void WriteParamsSet(ParamsSetViewModel paramsSet, TextWriter writer) {
        bool isAndOperator = paramsSet.SelectedOperator?.IsAndOperator ?? true;
        writer.WriteLine(isAndOperator ? "<div class=\"set\">" : "<div class=\"set or\">");
        writer.Write("<div class=\"op\">");
        WriteEncoded(paramsSet.SelectedOperator?.Name, writer);
        writer.WriteLine("</div>");
        foreach(var paramRule in paramsSet.InnerParamRules) {
            writer.Write("<div class=\"param\"><span class=\"name\">");
            WriteEncoded(paramRule.ParamName, writer);
            writer.Write("</span> ");
            WriteEncoded(paramRule.SelectedOperator?.Name, writer);
            if(paramRule.ExpectedValueNeeded) {
                writer.Write(' ');
                WriteEncoded(paramRule.ExpectedValue, writer);
            }

            writer.WriteLine("</div>");
        }

        foreach(var innerSet in paramsSet.InnerParamSets) {
            WriteParamsSet(innerSet, writer);
        }

        writer.WriteLine("</div>");
    }

    private void WriteEncoded(string value, TextWriter writer) {
        HttpUtility.HtmlEncode(value ?? string.Empty, writer);
    }
}
