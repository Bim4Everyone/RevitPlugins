using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models.ReportElements;

namespace RevitMarkPlacement.ViewModels.ReportViewModels;

internal class ReportElementViewModel : BaseViewModel {
    private readonly ReportElement _reportElement;

    public ReportElementViewModel() {
        _reportElement = null;
    }
    

    public ReportElementViewModel(ReportElement reportElement) {
        _reportElement = reportElement;

        DescriptionId = reportElement.Description.Id;
        DescriptionTitle = reportElement.Description.Title;
        FormattedMessage = reportElement.FormattedMessage;
        DescriptionDescription = reportElement.Description.Description;
        DescriptionReportLevel = reportElement.Description.ReportLevel;
    }

    public string DescriptionId { get; }
    public string DescriptionTitle { get; }
    public string FormattedMessage { get; }
    public string DescriptionDescription { get; }
    public ReportLevel DescriptionReportLevel { get; }
}
