using System.Linq;
using System.Windows;

using dosymep.SimpleServices;

using Ninject;
using Ninject.Syntax;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.ViewModels;
using RevitMarkAllDocuments.Views;

namespace RevitMarkAllDocuments.Services;

internal class WindowsService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly RevitRepository _revitRepository;
    private readonly DocumentService _documentService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILocalizationService _localizationService;

    public WindowsService(IResolutionRoot resolutionRoot,
                          RevitRepository revitRepository,
                          DocumentService documentService,
                          IMessageBoxService messageBoxService,
                          ILocalizationService localizationService) {
        _resolutionRoot = resolutionRoot;
        _revitRepository = revitRepository;
        _documentService = documentService;
        _messageBoxService = messageBoxService;
        _localizationService = localizationService;
    }

    public bool ShowMarkListWindow(MarkData markData) {
        var window = _resolutionRoot.Get<MarkListWindow>();        
        var markListViewModel = 
            new MarkListViewModel(markData, _revitRepository, _documentService, _localizationService);

        if(!markListViewModel.MarkedElements.Any()) {
            _messageBoxService.Show(_localizationService.GetLocalizedString("MessageBox.NoElements"),
                                    _localizationService.GetLocalizedString("MessageBox.Title"));
            return false;
        }

        window.DataContext = markListViewModel;

        window.ShowDialog();
        if((bool) window.DialogResult) {
            return true;
        }

        return false;
    }

    public bool ShowWarningsWindow(WarningsViewModel warnings) {
        if(warnings.Warnings.Any()) {
            var window = _resolutionRoot.Get<WarningsWindow>();
            window.DataContext = warnings;

            window.ShowDialog();

            return true;
        }

        return false;
    }
}
