using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels;

internal class DocumentViewModel : BaseViewModel {
    private string _name;
    private Document _document;
    private string _documentType;
    private bool _isChecked;

    public DocumentViewModel(string name, string documentType, bool isChecked = false) {
        _name = name;
        _documentType = documentType;
        _isChecked = isChecked;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public Document Document {
        get => _document;
        set => RaiseAndSetIfChanged(ref _document, value);
    }

    public string DocumentType {
        get => _documentType;
        set => RaiseAndSetIfChanged(ref _documentType, value);
    }

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

}
