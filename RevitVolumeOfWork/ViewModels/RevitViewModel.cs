using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            SetWallParametersCommand = new RelayCommand(SetWallParameters, CanSetWallParameters);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels()
                .OrderBy(item => item.Element.Elevation));
            //.Where(item => item.Rooms.Count > 0));
        }

        public ICommand SetWallParametersCommand { get; }

        public string Name { get; set; }

        public ObservableCollection<LevelViewModel> Levels { get; }

        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();

        private void SetWallParameters(object p) {
            List<RoomElement> rooms = Levels.Where(item => item.IsSelected)
                .SelectMany(x => x.Rooms)
                .ToList();

            Dictionary<int, WallElement> allWalls = _revitRepository.GetGroupedRoomsByWalls(rooms);

            using(Transaction t = _revitRepository.Document.StartTransaction("Заполнить параметры ВОР")) {
                foreach(var key in allWalls.Keys) {

                    var wallElement = allWalls[key];
                    var wall = wallElement.Wall;

                    wall.LookupParameter("Имя помещения").Set(wallElement.GetRoomsParameters("Name"));
                    wall.LookupParameter("Номер помещения").Set(wallElement.GetRoomsParameters("Number"));
                    wall.LookupParameter("ID помещения").Set(wallElement.GetRoomsParameters("ID"));
                    wall.LookupParameter("Номер квартиры").Set(wallElement.GetRoomsParameters("ApartNumber"));
                }
                t.Commit();
            }
        }

        private bool CanSetWallParameters(object p) {          
            return true; 
        }
    }
}
