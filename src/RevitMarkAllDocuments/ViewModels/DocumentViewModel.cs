using System.Windows.Controls;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels;

internal class DocumentViewModel : BaseViewModel {
    private readonly string _name;
    private readonly Document _document;
    private readonly string _documentType;
    private readonly bool _isLink;

    private bool _isChecked;

    public DocumentViewModel(string name, bool isLink, bool isChecked = false) {
        _name = name;
        _isLink = isLink;
        _documentType = isLink ? "Связь" : "Текущий",;
        _isChecked = isChecked;
    }

    public string Name => _name;
    public Document Document => _document;
    public bool IsLink => _isLink;
    public string DocumentType => _documentType;

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

}
