using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class RevitDocumentViewModel : BaseViewModel {
    private readonly DeclarationSettings _settings;
    private readonly ILocalizationService _localizationService;
    private bool _isChecked;

    public RevitDocumentViewModel(Document document, 
                                  DeclarationSettings settings, 
                                  ILocalizationService localizationService) {
        Document = document;
        _settings = settings;
        _localizationService = localizationService;
        Name = _localizationService.GetLocalizedString("MainWindow.FileName", Document.Title);

        Room = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .ToElements()
            .Cast<Room>()
            .FirstOrDefault();
    }

    public RevitDocumentViewModel(RevitLinkInstance revitLinkInstance, DeclarationSettings settings) {
        Document = revitLinkInstance.GetLinkDocument();
        _settings = settings;
        Name = _localizationService.GetLocalizedString("MainWindow.FileNameLink", revitLinkInstance.Name);

        Room = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .ToElements()
            .Cast<Room>()
            .FirstOrDefault();
    }

    public Document Document { get; }
    public string Name { get; }
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
    public Room Room { get; }

    public bool HasPhase(Phase phase) {
        bool checkPhase = Document
            .Phases
            .OfType<Phase>()
            .Select(x => x.Name)
            .Contains(phase.Name);

        return checkPhase;
    }

    public bool HasRooms() {
        return Room != null;
    }

    public WarningViewModel CheckParameters() {
        var errorListVM = new WarningViewModel(_localizationService) {
            WarningType = _localizationService.GetLocalizedString("WarningWindow.Error"),
            Description = _localizationService.GetLocalizedString("WarningsWindow.NoParamsInProjects"),
            DocumentName = Name,
            Elements = _settings
                .AllParameters
                .Select(x => x.Definition.Name)
                .Where(x => !Room.IsExistsParam(x))
                .Select(x => new WarningElementViewModel(x,
                    _localizationService.GetLocalizedString("WarningsWindow.NoParamInProject")))
                .ToList()
        };

        return errorListVM;
    }
}
