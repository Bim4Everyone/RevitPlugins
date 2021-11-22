using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.ViewModels.Revit.RoomsNums;

namespace RevitRooms.ViewModels {
    internal class RoomNumsViewModel : BaseViewModel {
        private RoomsNumsViewModel _roomsNums;

        public RoomNumsViewModel(Application application, Document document) {
            RoomsNumsViewModels = new ObservableCollection<RoomsNumsViewModel> {
                new ViewRevitViewModel(application, document) { Name = "Выборка по текущему виду" },
                new ElementsRevitViewModel(application, document) { Name = "Выборка по всем элементам" },
                new SelectedRevitViewModel(application, document) { Name = "Выборка по выделенным элементам" }
            };

            RoomsNums = RoomsNumsViewModels[1];

            var roomsConfig = RoomsNumsConfig.GetConfig();
            var settings = roomsConfig.GetRoomsNumsSettingsConfig(document.Title);
            if(settings != null) {
                RoomsNums = RoomsNumsViewModels.FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RoomsNums;
            }
        }

        public RoomsNumsViewModel RoomsNums {
            get => _roomsNums;
            set => this.RaiseAndSetIfChanged(ref _roomsNums, value);
        }

        public ObservableCollection<RoomsNumsViewModel> RoomsNumsViewModels { get; }
    }
}
