using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Запись замечаний сверки Excel в один раздел реестра.
/// </summary>
internal class FindingWriter {
    private readonly List<Finding> _findings;
    private readonly string _section;
    private readonly string _title;

    public FindingWriter(List<Finding> findings, string section, string title) {
        _findings = findings;
        _section = section;
        _title = title;
    }

    public void Add(string text, string kind = FindingKinds.Remark, FindingTable? table = null) {
        _findings.Add(new Finding {
            Section = _section,
            Title = _title,
            Text = text,
            Kind = kind,
            IsFromExcel = true,
            Table = table
        });
    }
}
