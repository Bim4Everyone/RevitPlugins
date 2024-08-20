using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitFinishing.ViewModels;

namespace RevitFinishing.Models.Finishing {
    internal class FinishingChecker {
        private readonly string _phaseName;

        public FinishingChecker(Phase phase) {
            _phaseName = phase.Name;
        }

        public List<ErrorElement> CheckFinishingByRoomBounding(FinishingInProject finishings) {
            return finishings
                .AllFinishing
                .Where(x => x.GetParamValueOrDefault(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 0) == 1)
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        public List<ErrorElement> CheckRoomsByKeyParameter(IEnumerable<Element> rooms, string paramName) {
            return rooms
                .Where(x => x.GetParam(paramName).AsElementId() == ElementId.InvalidElementId)
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        public List<ErrorElement> CheckRoomsByParameter(IEnumerable<Element> rooms, string paramName) {
            return rooms
                .Where(x => string.IsNullOrEmpty(x.GetParamValue<string>(paramName)))
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        public List<ErrorElement> CheckFinishingByRoom(IEnumerable<FinishingElement> finishings) {
            return finishings
                .Where(x => !x.CheckFinishingTypes())
                .Select(x => x.RevitElement)
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }
    }
}
