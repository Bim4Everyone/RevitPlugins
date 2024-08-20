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
using RevitRooms.ViewModels.Revit.RoomsNums;
using RevitRooms.Views;

namespace RevitRooms.ViewModels {
    internal class RoomNumsViewModel : BaseViewModel {
        private RoomsNumsViewModel _roomsNums;

        public RoomNumsViewModel(RevitRepository revitRepository, RoomsNumsWindows window) {
            RoomsNumsViewModels = new ObservableCollection<RoomsNumsViewModel> {
                new ViewRevitViewModel(revitRepository) { Name = "Выборка по текущему виду", ParentWindow = window },
                new ElementsRevitViewModel(revitRepository) { Name = "Выборка по всем элементам", ParentWindow = window },
                new SelectedRevitViewModel(revitRepository) { Name = "Выборка по выделенным элементам", ParentWindow = window }
            };

            RoomsNums = RoomsNumsViewModels[1];

            var roomsConfig = RoomsNumsConfig.GetPluginConfig();
            var settings = roomsConfig.GetSettings(revitRepository.DocumentName);
            if(settings != null) {
                RoomsNums = RoomsNumsViewModels.FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RoomsNums;
            }
        }

        public RoomsNumsViewModel RoomsNums {
            get => _roomsNums;
            set => this.RaiseAndSetIfChanged(ref _roomsNums, value);
        }

        public ObservableCollection<RoomsNumsViewModel> RoomsNumsViewModels { get; }
        public string NumberParamName => LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER);
        public string ApartmentNumberParamName => SharedParamsConfig.Instance.ApartmentNumber.Name;
    }
}
