using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class RoomGroupViewModel : BaseViewModel {

        private readonly string _name;
        private readonly IReadOnlyCollection<RoomFromRevit> _rooms;
        private readonly IReadOnlyCollection<Apartment> _apartments;

        private bool _isChecked;

        public RoomGroupViewModel(string name, IEnumerable<RoomFromRevit> rooms) {
            _name = name;
            _rooms = rooms.ToList();

            RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;
            _apartments = _rooms
                .GroupBy(r => r.RoomObject.GetParamValueOrDefault(sectionParam, "<Без секции>"))
                .Select(x => new Apartment(x))
                .ToList();
        }

        public string Name => _name;
        public IReadOnlyCollection<RoomFromRevit> Rooms => _rooms;
        public IReadOnlyCollection<Apartment> Apartments => _apartments;

        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);
        }
    }
}
