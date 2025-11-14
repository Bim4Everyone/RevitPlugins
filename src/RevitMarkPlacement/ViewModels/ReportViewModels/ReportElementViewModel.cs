using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models.ReportElements;

namespace RevitMarkPlacement.ViewModels.ReportViewModels;

internal class ReportElementViewModel : BaseViewModel {
    private readonly ReportDescription _reportDescription;

    public ReportElementViewModel() {
        _reportDescription = null;
    }


    public ReportElementViewModel(ReportDescription reportDescription, IEnumerable<string> messages) {
        _reportDescription = reportDescription;

        Id = _reportDescription.Id;
        Title = _reportDescription.Title;
        Description = _reportDescription.Description;
        ReportLevel = _reportDescription.ReportLevel;

        Messages = [..messages];
    }

    public string Id { get; }
    public string Title { get; }
    public string Description { get; }
    public ReportLevel ReportLevel { get; }
    public ObservableCollection<string> Messages { get; }
}
