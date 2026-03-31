using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Ninject;
using Ninject.Syntax;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.ViewModels;
using RevitMarkAllDocuments.Views;

namespace RevitMarkAllDocuments.Services;

internal class MarkListWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public MarkListWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public bool ShowWindow(Document document, IList<MarkedElement> markedElements) {
        var window = _resolutionRoot.Get<MarkListWindow>();
        window.DataContext = new MarkListViewModel(document, markedElements);

        window.ShowDialog();
        if((bool) window.DialogResult) {
            return true;
        }

        return false;
    }
}
