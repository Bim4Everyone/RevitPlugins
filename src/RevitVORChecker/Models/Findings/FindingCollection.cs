using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Findings;

/// <summary>
/// Накопитель замечаний: одинаковые замечания (раздел, вид, текст) по разным файлам объединяются в одно.
/// </summary>
internal class FindingCollection {
    private readonly OrderedMap<string, Finding> _findings = new();

    public List<Finding> ToList() {
        return _findings.Values.ToList();
    }

    public void Add(
        string section,
        string text,
        IEnumerable<long>? elementIds = null,
        string? fileName = null,
        string kind = FindingKinds.Remark,
        string? detail = null) {

        string key = section + "\u0001" + kind + "\u0001" + text;
        Finding finding = _findings.GetOrAdd(key, () => new Finding {
            Section = section,
            Title = ReportSections.GetTitle(section),
            Kind = kind,
            Text = text
        });
        finding.Entries.Add(new FindingEntry {
            FileName = fileName,
            Detail = detail,
            ElementIds = (elementIds ?? Enumerable.Empty<long>()).Distinct().OrderBy(id => id).ToList()
        });
    }
}
