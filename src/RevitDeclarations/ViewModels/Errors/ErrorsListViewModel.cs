using System.Collections.Generic;

using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels;
internal class ErrorsListViewModel : BaseViewModel {
    public string ErrorType { get; set; }
    public string Description { get; set; }
    public string DocumentName { get; set; }
    public string FullErrorName => $"{ErrorType} в проекте \"{DocumentName}\"";

    public IList<ErrorElement> Errors { get; set; } = [];
}

public class ErrorElement {
    public ErrorElement(string elementInfo, string errorInfo) {
        ElementInfo = elementInfo;
        ErrorInfo = errorInfo;
    }

    public string ElementInfo { get; }
    public string ErrorInfo { get; }
}
