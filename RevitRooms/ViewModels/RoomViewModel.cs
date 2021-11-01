using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitRooms.ViewModels {
    internal class RoomViewModel : BaseViewModel, IEquatable<RoomViewModel> {
        private readonly Room _room;

        public RoomViewModel(Room room, Phase phase) {
            _room = room;
            Phase = new PhaseViewModel(phase);

            if(RoomArea == null || RoomArea == 0) {
                var segments = _room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                IsRedundant = segments.Count > 0;
                NotEnclosed = segments.Count == 0;
            }
        }

        public string DisplayData {
            get { return _room.Name; }
        }

        public Element RoomTypeGroup {
            get { return GetParamElement(ProjectParamsConfig.Instance.RoomTypeGroupName); }
        }

        public Element Room {
            get { return GetParamElement(ProjectParamsConfig.Instance.RoomName); }
        }

        public Element RoomGroup {
            get { return GetParamElement(ProjectParamsConfig.Instance.RoomGroupName); }
        }

        public Element RoomSection {
            get { return GetParamElement(ProjectParamsConfig.Instance.RoomSectionName); }
        }

        public string LevelName {
            get { return _room.Level.Name; }
        }

        public double? RoomArea {
            get { return (double?) _room.GetParamValueOrDefault(BuiltInParameter.ROOM_AREA); }
        }

        public bool IsPlaced {
            get { return _room.Location != null; }
        }

        public bool? IsRedundant { get; }
        public bool? NotEnclosed { get; }
        public PhaseViewModel Phase { get; }

        public void UpdateSharedParams() {
            _room.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaSpec, ProjectParamsConfig.Instance.ApartmentAreaSpec);
            _room.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaMinSpec, ProjectParamsConfig.Instance.ApartmentAreaMinSpec);
            _room.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaMaxSpec, ProjectParamsConfig.Instance.ApartmentAreaMaxSpec);

            _room.SetParamValue(SharedParamsConfig.Instance.RoomGroupShortName, ProjectParamsConfig.Instance.RoomGroupShortName);
            _room.SetParamValue(SharedParamsConfig.Instance.RoomSectionShortName, ProjectParamsConfig.Instance.RoomSectionShortName);
            _room.SetParamValue(SharedParamsConfig.Instance.RoomTypeGroupShortName, ProjectParamsConfig.Instance.RoomTypeGroupShortName);
            _room.SetParamValue(SharedParamsConfig.Instance.FireCompartmentShortName, ProjectParamsConfig.Instance.FireCompartmentShortName);

            _room.SetParamValue(SharedParamsConfig.Instance.Level, _room.Level.Name.Replace(" этаж", string.Empty));
        }

        public override bool Equals(object obj) {
            return Equals(obj as RoomViewModel);
        }

        public bool Equals(RoomViewModel other) {
            return other != null && _room.Id == other._room.Id;
        }

        public override int GetHashCode() {
            return -1737854931 + EqualityComparer<Room>.Default.GetHashCode(_room);
        }

        private Element GetParamElement(RevitParam revitParam) {
            ElementId elementId = (ElementId) _room.GetParamValueOrDefault(revitParam);
            return elementId == null ? null : _room.Document.GetElement(elementId);
        }
    }
}
