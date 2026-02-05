using RevitMarkPlacement.ViewModels;

namespace RevitMarkPlacement.Models.ReportElements;

internal sealed class ReportElement {
    public ReportElement(ReportDescription description, params object[] args) {
        Description = description;
        FormattedMessage = string.Format(Description.MessageFormat, args);
    }

    public string FormattedMessage { get; }
    public ReportDescription Description { get; }
}
