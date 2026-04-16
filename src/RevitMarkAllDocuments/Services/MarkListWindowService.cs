using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

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

    public bool ShowWindow(MarkData markData, 
                           RevitRepository revitRepository,
                           DocumentService documentService,
                           ILocalizationService localizationService) {
        var window = _resolutionRoot.Get<MarkListWindow>();
        window.DataContext = new MarkListViewModel(markData, 
                                                   revitRepository, 
                                                   documentService,
                                                   localizationService);

        window.ShowDialog();
        if((bool) window.DialogResult) {
            return true;
        }

        return false;
    }
}
