using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitBuildCoordVolumes.ViewModels;

internal class DocumentViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private string _sourceFileUniqueId;
    private string _name;
    private bool _isChecked;

    public DocumentViewModel(ILocalizationService localizationService, Document document) {
        _localizationService = localizationService;
        Document = document;
        DocumentUniqueId = document.GetUniqId();
        Name = GetFileName();
    }

    public Document Document { get; set; }

    public string DocumentUniqueId {
        get => _sourceFileUniqueId;
        set => RaiseAndSetIfChanged(ref _sourceFileUniqueId, value);
    }
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    // Метод получения имени файла
    private string GetFileName() {
        return !Document.IsLinked
           ? _localizationService.GetLocalizedString("DocumentViewModel.CurrentFile")
           : DocumentUniqueId;
    }
}
