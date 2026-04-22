using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class DocumentsPageViewModel : BaseViewModel {
    private readonly ObservableCollection<DocumentViewModel> _documents;

    public DocumentsPageViewModel(RevitRepository repository) {
        _documents = [.. repository.GetAllDocuments().Select(doc => new DocumentViewModel(doc))];
    }

    public ObservableCollection<DocumentViewModel> Documents => _documents;
}
