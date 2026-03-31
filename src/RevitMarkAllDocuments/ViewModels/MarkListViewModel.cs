using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class MarkListViewModel : BaseViewModel {
    private ObservableCollection<MarkedElementViewModel> _markedElements;

    public MarkListViewModel(Document document, IList<MarkedElement> elements) {
        _markedElements = [..elements
            .Select(x => new MarkedElementViewModel(x, document))
            .ToList()];

        MarkElementsCommand = RelayCommand.Create(MarkElements);
    }

    public ICommand MarkElementsCommand { get; }

    public ObservableCollection<MarkedElementViewModel> MarkedElements {
        get => _markedElements;
        set => RaiseAndSetIfChanged(ref _markedElements, value);
    }

    private void MarkElements() {
        
    }
}
