using System.Windows.Controls;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels;

internal class DocumentViewModel : BaseViewModel {
    private readonly string _name;
    private readonly Document _document;
    private readonly string _documentType;
    private readonly bool _isLink;

    private bool _isChecked;

    public DocumentViewModel(Document document, ILocalizationService localizationService) {
        _document = document;
        _name = document.Title;
        _isLink = document.IsLinked;
        _documentType = _isLink 
            ? localizationService.GetLocalizedString("MainWindow.DocLink")
            : localizationService.GetLocalizedString("MainWindow.DocActive");
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
