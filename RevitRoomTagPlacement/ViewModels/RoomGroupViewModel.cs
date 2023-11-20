using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class RoomGroupViewModel : BaseViewModel {

        private readonly string _name;
        private readonly IEnumerable<RoomFromRevit> _rooms;
        private readonly IEnumerable<Apartment> _apartments;

        public RoomGroupViewModel(string name, IEnumerable<RoomFromRevit> rooms) {
            _name = name;
            _rooms = rooms;

            RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;
            _apartments = rooms
                .GroupBy(r => r.RoomObject.GetParamValueOrDefault(sectionParam, "<Без секции>"))
                .Select(x => new Apartment(x));
        }

        public string Name => _name;
        public IEnumerable<RoomFromRevit> Rooms => _rooms;
        public IEnumerable<Apartment> Apartments => _apartments;

        private bool isChecked;
        public bool IsChecked {
            get => isChecked;
            set => RaiseAndSetIfChanged(ref isChecked, value);
        }
    }
}
