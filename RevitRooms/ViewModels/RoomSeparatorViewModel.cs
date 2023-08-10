using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal sealed class RoomSeparatorViewModel : ElementViewModel<CurveElement> {
        private readonly List<SpatialElementViewModel> _rooms = new List<SpatialElementViewModel>();

        public RoomSeparatorViewModel(CurveElement element, PhaseViewModel phase, RevitRepository revitRepository)
            : base(element, revitRepository) {
            Phase = phase;
        }

        public PhaseViewModel Phase { get; }
        public ElementId LevelId => Element.LevelId;
        public override string LevelName => RevitRepository.GetElement(Element.LevelId)?.Name;

        public override string PhaseName {
            get { return Phase.Name; }
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
                    .Count() <= 1; // может не быть квартир и будет значение 0
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
                    .Count() <= 1; // может не быть квартир и будет значение 0
            }
        }
    }
}