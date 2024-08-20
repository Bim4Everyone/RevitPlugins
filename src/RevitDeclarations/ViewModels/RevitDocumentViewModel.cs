using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RevitDeclarations.Models;

using dosymep.WPF.ViewModels;
using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitDeclarations.ViewModels {
    internal class RevitDocumentViewModel : BaseViewModel {
        private readonly Document _document;
        private readonly DeclarationSettings _settings;

        private readonly string _name;
        private readonly Room _room;

        private bool _isChecked;

        public RevitDocumentViewModel(Document document, DeclarationSettings settings) {
            _document = document;
            _settings = settings;
            _name = $"{_document.Title} [текущий проект]";

            _room = new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .ToElements()
                .Cast<Room>()
                .FirstOrDefault();
        }

        public RevitDocumentViewModel(RevitLinkInstance revitLinkInstance, DeclarationSettings settings) {
            _document = revitLinkInstance.GetLinkDocument();
            _settings = settings;
            _name = $"{revitLinkInstance.Name} [связь]";

            _room = new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .ToElements()
                .Cast<Room>()
                .FirstOrDefault();
        }

        public Document Document => _document;
        public string Name => _name;
        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);
        }
        public Room Room => _room;

        public bool HasPhase(Phase phase) {
            bool checkPhase = _document
                .Phases
                .OfType<Phase>()
                .Select(x => x.Name)
                .Contains(phase.Name);

            if(checkPhase) {
                return true;
            }
            return false;
        }

        public bool HasRooms() {
            if(_room != null) {
                return true;
            }
            return false;
        }

        public ErrorsListViewModel CheckParameters() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Ошибка",
                Description = "В проекте отсутствует параметр, выбранный в исходных данных",
                DocumentName = _name
            };

            //RoomForChecks roomForCheck = new RoomForChecks(_room);

            errorListVM.Errors = _settings
                .AllParameters
                .Select(x => x.Definition.Name)
                .Where(x => !_room.IsExistsParam(x))
                .Select(x => new ErrorElement(x, "Отсутствует параметр в проекте"))
                .ToList();

            return errorListVM;
        }
    }
}
