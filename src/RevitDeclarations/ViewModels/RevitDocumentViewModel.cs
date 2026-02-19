using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class RevitDocumentViewModel : BaseViewModel {
    private readonly DeclarationSettings _settings;
    private bool _isChecked;

    public RevitDocumentViewModel(Document document, DeclarationSettings settings) {
        Document = document;
        _settings = settings;
        Name = $"{Document.Title} [текущий проект]";

        Room = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .ToElements()
            .Cast<Room>()
            .FirstOrDefault();
    }

    public RevitDocumentViewModel(RevitLinkInstance revitLinkInstance, DeclarationSettings settings) {
        Document = revitLinkInstance.GetLinkDocument();
        _settings = settings;
        Name = $"{revitLinkInstance.Name} [связь]";

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

    public ErrorsListViewModel CheckParameters() {
        var errorListVM = new ErrorsListViewModel {
            ErrorType = "Ошибка",
            Description = "В проекте отсутствует параметр, выбранный в исходных данных",
            DocumentName = Name,
            Errors = _settings
                .AllParameters
                .Select(x => x.Definition.Name)
                .Where(x => !Room.IsExistsParam(x))
                .Select(x => new ErrorElement(x, "Отсутствует параметр в проекте"))
                .ToList()
        };

        return errorListVM;
    }
}
