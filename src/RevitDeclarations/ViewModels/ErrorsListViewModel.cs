using System.Collections.Generic;

using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels {
    internal class ErrorsListViewModel : BaseViewModel {
        public string Message { get; set; }
        public string Description { get; set; }
        public string DocumentName { get; set; }

        public IList<ErrorElement> Errors { get; set; } = new List<ErrorElement>();
    }

    public class ErrorElement {
        private readonly string _elementInfo;
        private readonly string _errorInfo;

        public ErrorElement(string elementInfo, string errorInfo) {
            _elementInfo = elementInfo;
            _errorInfo = errorInfo;
        }

        public string ElementInfo => _elementInfo;
        public string ErrorInfo => _errorInfo;
    }
}
