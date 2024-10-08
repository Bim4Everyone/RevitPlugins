using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class DeclarationProject {
        private protected readonly RevitDocumentViewModel _document;
        private protected readonly DeclarationSettings _settings;
        private protected readonly RevitRepository _revitRepository;

        private protected readonly Phase _phase;

        private protected readonly IEnumerable<RoomElement> _rooms;

        public DeclarationProject(RevitDocumentViewModel document,
                                  RevitRepository revitRepository,
                                  DeclarationSettings settings) {
            _document = document;
            _settings = settings;
            _revitRepository = revitRepository;

            _phase = revitRepository.GetPhaseByName(document.Document, _settings.SelectedPhase.Name);

            _rooms = revitRepository.GetRoomsOnPhase(document.Document, _phase, settings);
            _rooms = FilterDeclarationRooms(_rooms);
        }

        /// <summary>Отфильтрованные помещения для выгрузки</summary>
        public IEnumerable<RoomElement> Rooms => _rooms;
        public RevitDocumentViewModel Document => _document;
        public Phase Phase => _phase;

        private IReadOnlyCollection<RoomElement> FilterDeclarationRooms(IEnumerable<RoomElement> rooms) {
            Parameter filterParam = _settings.FilterRoomsParam;
            string[] filterValues = _settings.FilterRoomsValue.Split(',');
            StringComparer strComparer = StringComparer.OrdinalIgnoreCase;

            return rooms
                .Where(x => filterValues.Contains(x.GetTextParamValue(filterParam), strComparer))
                .ToList();
        }
    }
}
