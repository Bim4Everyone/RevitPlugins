using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.ViewModels.Revit;

namespace RevitRooms.ViewModels {
    internal class RoomsViewModel : BaseViewModel {
        private RevitViewModel _revitViewModel;

        public RoomsViewModel(RevitRepository revitRepository) {
            RevitViewModels = new ObservableCollection<RevitViewModel> {
                new ViewRevitViewModel(revitRepository) { Name = "Выборка по текущему виду" },
                new ElementsRevitViewModel(revitRepository) { Name = "Выборка по всем элементам" },
                new SelectedRevitViewModel(revitRepository) { Name = "Выборка по выделенным элементам" }
            };

            RevitViewModel = RevitViewModels[1];

            var roomsConfig = RoomsConfig.GetRoomsConfig();
            var settings = roomsConfig.GetSettings(revitRepository.Document);
            if(settings != null) {
                RevitViewModel = RevitViewModels.FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RevitViewModel;
            }
        }

        public RevitViewModel RevitViewModel {
            get => _revitViewModel;
            set => this.RaiseAndSetIfChanged(ref _revitViewModel, value);
        }

        public ObservableCollection<RevitViewModel> RevitViewModels { get; }
        public string LevelParamName => SharedParamsConfig.Instance.Level.Name;
    }
}
