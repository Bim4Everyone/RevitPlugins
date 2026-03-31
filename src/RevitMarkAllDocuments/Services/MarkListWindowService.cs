using System.Collections.Generic;
using System.Linq;

using Ninject;
using Ninject.Syntax;

using RevitMarkAllDocuments.ViewModels;
using RevitMarkAllDocuments.Views;

namespace RevitMarkAllDocuments.Services;

internal class MarkListWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public MarkListWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public bool ShowWindow(IList<MarkedElementViewModel> markedElements) {
        var window = _resolutionRoot.Get<MarkListWindow>();
        window.DataContext = new MarkListViewModel() {
            MarkedElements = [.. markedElements]
        };

        window.ShowDialog();
        if((bool) window.DialogResult) {
            return true;
        }

        return false;
    }
}
