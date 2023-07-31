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

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _isAllowSelectLevels;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            SetWallParametersCommand = new RelayCommand(SetWallParameters, CanSetWallParameters);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels()
                .OrderBy(item => item.Element.Elevation));
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

                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomName.Name,
                                                wallElement.GetRoomsParameters("Name"));
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomNumber.Name,
                                                wallElement.GetRoomsParameters("Number"));
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomID.Name,
                                                wallElement.GetRoomsParameters("ID"));
                    wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedApartmentNumber.Name,
                                                wallElement.GetRoomsParameters("ApartNumber"));
                }
                t.Commit();
            }
        }

        private bool CanSetWallParameters(object p) {
            if(!_revitRepository.Document.IsExistsSharedParam(SharedParamsConfig.Instance.ApartmentNumber.Name)) {
                ErrorText = "У помещений отсутствует параметр номера квартиры";
                return false;
            }
            return true;
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool IsAllowSelectLevels {
            get => _isAllowSelectLevels;
            set => this.RaiseAndSetIfChanged(ref _isAllowSelectLevels, value);
        }
    }
}
