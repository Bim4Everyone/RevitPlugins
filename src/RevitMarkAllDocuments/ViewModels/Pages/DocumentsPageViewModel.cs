using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class DocumentsPageViewModel : BaseViewModel {
    private readonly ObservableCollection<DocumentViewModel> _documents;

    public DocumentsPageViewModel(RevitRepository repository, ILocalizationService localizationService) {
        _documents = [.. repository.GetAllDocuments().Select(doc => new DocumentViewModel(doc, localizationService))];
    }

    public ObservableCollection<DocumentViewModel> Documents => _documents;
}
