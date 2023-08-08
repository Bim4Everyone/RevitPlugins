using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal sealed class RoomSeparatorViewModel : ElementViewModel<CurveElement> {
        private readonly SpatialElementViewModel[] _rooms;

        public RoomSeparatorViewModel(CurveElement element,
            RevitRepository revitRepository,
            IEnumerable<SpatialElementViewModel> rooms)
            : base(element, revitRepository) {
            _rooms = rooms.ToArray();
        }

        public bool IsSectionNameEqual {
            get {
                if(_rooms.Length == 0
                   || _rooms.Length == 1) {
                    return true;
                }
                
                return _rooms
                    .Select(item => item.RoomSection?.Id)
                    .Distinct()
                    .Count() == 1;
            }
        }

        public bool IsGroupNameEqual {
            get {
                if(_rooms.Length == 0
                   || _rooms.Length == 1) {
                    return true;
                }

                return _rooms
                    .Where(item =>
                        item.RoomGroup?.Name.IndexOf("квартира", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .Select(item => item.RoomGroup?.Id)
                    .Distinct()
                    .Count() == 1;
            }
        }
    }
}