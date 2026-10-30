using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks;

/// <summary>
/// Элементы, сгруппированные по тексту замечания и уточнению, в порядке появления групп.
/// </summary>
internal class GroupedElements {
    private readonly OrderedMap<string, ElementGroup> _groups = new();

    public void Add(string text, string? detail, ElementData element) {
        string key = text + "\u0001" + (detail ?? "\u0002");
        _groups.GetOrAdd(key, () => new ElementGroup(text, detail)).Elements.Add(element);
    }

    /// <summary>
    /// Добавляет замечания по группам: уточнение группы и перечень типоразмеров.
    /// </summary>
    public void AddFindings(
        FindingCollection findings,
        string section,
        string fileName,
        int maxTypeCount = 3,
        string kind = FindingKinds.Remark) {

        foreach(ElementGroup group in _groups.Values) {
            string detailPrefix = group.Detail != null ? group.Detail + "; " : "";
            findings.Add(
                section,
                group.Text,
                group.Elements.Select(element => element.Id),
                fileName,
                kind,
                detailPrefix + ModelCheckContext.DescribeTypes(group.Elements, maxTypeCount));
        }
    }

    private class ElementGroup {
        public ElementGroup(string text, string? detail) {
            Text = text;
            Detail = detail;
        }

        public string Text { get; }

        public string? Detail { get; }

        public List<ElementData> Elements { get; } = new();
    }
}
