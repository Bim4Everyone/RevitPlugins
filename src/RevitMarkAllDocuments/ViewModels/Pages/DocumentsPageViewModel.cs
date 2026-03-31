using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class DocumentsPageViewModel : BaseViewModel {

    public DocumentsPageViewModel(RevitRepository repository) {
        Documents = new ObservableCollection<DocumentViewModel>(
            repository.GetAllDocuments()
                .Select(doc => new DocumentViewModel(doc))
        );

    }
    public ObservableCollection<DocumentViewModel> Documents { get; }
}
