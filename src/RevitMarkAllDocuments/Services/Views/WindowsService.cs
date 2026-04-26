using System.Linq;

using dosymep.Bim4Everyone;
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

    public bool ShowMarkListWindow(MarkDataByDocument markDataForCurrentDoc, RevitParam markParam) {
        var window = _resolutionRoot.Get<MarkListWindow>();
        var markListViewModel =
            new MarkListViewModel(markDataForCurrentDoc, markParam, _revitRepository, _localizationService);

        if(!markListViewModel.MarkedElements.Any()) {
            _messageBoxService.Show(_localizationService.GetLocalizedString("MessageBox.NoElements"),
                                    _localizationService.GetLocalizedString("MessageBox.Title"));
            return false;
        }

        window.DataContext = markListViewModel;

        window.ShowDialog();
        return (bool) window.DialogResult;
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
