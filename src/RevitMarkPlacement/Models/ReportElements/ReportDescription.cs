namespace RevitMarkPlacement.Models.ReportElements;

internal sealed class ReportDescription {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string MessageFormat { get; set; }

    public ReportLevel ReportLevel { get; set; }
}
