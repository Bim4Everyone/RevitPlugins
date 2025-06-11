using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitFinishing.ViewModels;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingChecker {
        private readonly Phase _phase;
        private readonly string _phaseName;

        public FinishingChecker(Phase phase) {
            _phase = phase;
            _phaseName = phase.Name;
        }

        public List<NoticeElementViewModel> CheckPhaseContainsFinishing(FinishingInProject finishings) {
            if(!finishings.AllFinishing.Any()) {
                return new List<NoticeElementViewModel>() {
                    new NoticeElementViewModel(_phase, _phaseName)
                };
            }

            return new List<NoticeElementViewModel>();
        }

        public List<NoticeElementViewModel> CheckFinishingByRoomBounding(FinishingInProject finishings) {
            return finishings
                .AllFinishing
                .Where(x => x.GetParamValueOrDefault(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 0) == 1)
                .Select(x => new NoticeElementViewModel(x, _phaseName))
                .ToList();
        }

        public List<NoticeElementViewModel> CheckRoomsByKeyParameter(IEnumerable<Element> rooms, string paramName) {
            return rooms
                .Where(x => x.GetParam(paramName).AsElementId() == ElementId.InvalidElementId)
                .Select(x => new NoticeElementViewModel(x, _phaseName))
                .ToList();
        }

        public List<NoticeElementViewModel> CheckRoomsByParameter(IEnumerable<Element> rooms, string paramName) {
            return rooms
                .Where(x => string.IsNullOrEmpty(x.GetParamValue<string>(paramName)))
                .Select(x => new NoticeElementViewModel(x, _phaseName))
                .ToList();
        }

        public List<NoticeElementViewModel> CheckFinishingByRoom(IEnumerable<FinishingElement> finishings) {
            return finishings
                .Where(x => !x.CheckFinishingTypes())
                .Select(x => x.RevitElement)
                .Select(x => new NoticeElementViewModel(x, _phaseName))
                .ToList();
        }
    }
}
