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
    internal class RoomViewModel : ElementViewModel<Room> {
        public RoomViewModel(Room room, RevitRepository revitRepository)
            : base(room, revitRepository) {
            Phase = new PhaseViewModel(revitRepository.GetPhase(room), revitRepository);

            if(RoomArea == null || RoomArea == 0) {
                var segments = Element.GetBoundarySegments(new SpatialElementBoundaryOptions());
                IsRedundant = segments.Count > 0;
                NotEnclosed = segments.Count == 0;
            }
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
            get { return Element.Level.Name; }
        }

        public double? RoomArea {
            get { return (double?) Element.GetParamValueOrDefault(BuiltInParameter.ROOM_AREA); }
        }

        public bool IsPlaced {
            get { return Element.Location != null; }
        }

        public bool? IsRedundant { get; }
        public bool? NotEnclosed { get; }
        public PhaseViewModel Phase { get; }

        public void UpdateSharedParams() {
            Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaSpec, ProjectParamsConfig.Instance.ApartmentAreaSpec);
            Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaMinSpec, ProjectParamsConfig.Instance.ApartmentAreaMinSpec);
            Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaMaxSpec, ProjectParamsConfig.Instance.ApartmentAreaMaxSpec);

            Element.SetParamValue(SharedParamsConfig.Instance.RoomGroupShortName, ProjectParamsConfig.Instance.RoomGroupShortName);
            Element.SetParamValue(SharedParamsConfig.Instance.RoomSectionShortName, ProjectParamsConfig.Instance.RoomSectionShortName);
            Element.SetParamValue(SharedParamsConfig.Instance.RoomTypeGroupShortName, ProjectParamsConfig.Instance.RoomTypeGroupShortName);
            Element.SetParamValue(SharedParamsConfig.Instance.FireCompartmentShortName, ProjectParamsConfig.Instance.FireCompartmentShortName);

            Element.SetParamValue(SharedParamsConfig.Instance.Level, Element.Level.Name.Replace(" этаж", string.Empty));
        }

        private Element GetParamElement(RevitParam revitParam) {
            ElementId elementId = (ElementId) Element.GetParamValueOrDefault(revitParam);
            return elementId == null ? null : Element.Document.GetElement(elementId);
        }
    }
}
