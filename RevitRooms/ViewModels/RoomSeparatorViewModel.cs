using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal sealed class RoomSeparatorViewModel : ElementViewModel<CurveElement> {
        private readonly List<SpatialElementViewModel> _rooms = new List<SpatialElementViewModel>();

        public RoomSeparatorViewModel(CurveElement element, RevitRepository revitRepository)
            : base(element, revitRepository) {
        }

        public void AddRoom(SpatialElementViewModel spatialElement) {
            if(spatialElement.BoundarySegments.Contains(ElementId)) {
                _rooms.Add(spatialElement);
            }
        }

        public void AddRooms(IEnumerable<SpatialElementViewModel> spatialElements) {
            foreach(SpatialElementViewModel spatialElement in spatialElements) {
                AddRoom(spatialElement);
            }
        }

        public bool IsSectionNameEqual {
            get {
                if(_rooms.Count == 0
                   || _rooms.Count == 1) {
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
                if(_rooms.Count == 0
                   || _rooms.Count == 1) {
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