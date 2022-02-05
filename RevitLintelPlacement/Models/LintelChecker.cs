using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {
    internal class LintelChecker {
        private readonly List<IChecker> _checkers;

        public LintelChecker(RevitRepository revitRepository, RulesSettings rulesSettings) {
            _checkers = new List<IChecker> {
            new LintelGroupChecker(),
            new ElementInWallChecker(revitRepository),
            new LintelElementInWallChecker(revitRepository, rulesSettings),
            new GeometricalLintelChecker(revitRepository)
            };
        }

        public IEnumerable<IResultHandler> Check(IEnumerable<LintelInfoViewModel> lintels) {
            foreach(var lintel in lintels) {
                foreach(var checker in _checkers) {
                    var resultHandler = checker.Check(lintel.Lintel, lintel.ElementInWall); 
                    resultHandler?.Handle();
                    if(resultHandler is LintelGeometricalDisplaced || resultHandler is LintelIsFixedWithoutElement)
                        yield return resultHandler;
                    if(!(resultHandler is null)) // null возвращается в случае, если перемычка по данной проверке установлена корректно
                        break;
                }
            }
        }
    }

    internal class LintelGroupChecker : IChecker {
        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel.GroupId != ElementId.InvalidElementId || lintel.SuperComponent != null)
                return new LintelInGroup();
            return null;
        }
    }

    internal class ElementInWallChecker : IChecker {
        private readonly RevitRepository _revitRepository;

        public ElementInWallChecker(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(elementInWall != null) {
                return null;
            }

            if((int) lintel.GetParamValue("Фиксировать") == 1) { //Todo: параметр "Фиксировать"
                return new LintelIsFixedWithoutElement(lintel.Id);
            }
            return new LintelWithoutElement(_revitRepository, lintel);
        }
    }

    internal class LintelElementInWallChecker : IChecker {
        private readonly GroupedRuleCollectionViewModel _groupedRules;
        private readonly RevitRepository _revitRepository;

        public LintelElementInWallChecker(RevitRepository revitRepository, RulesSettings rulesSettings) {
            this._revitRepository = revitRepository;
            this._groupedRules = new GroupedRuleCollectionViewModel(_revitRepository, rulesSettings);
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            var rule = _groupedRules.GetRule(elementInWall);
            if(rule == null) {
                return (int) lintel.GetParamValue("Фиксировать") == 1 ? null : new ElementInWallWithoutRule(_revitRepository, lintel);
            } else {
                //проверка корректности параметров перемычки
            }
            return null;
        }
    }

    internal class GeometricalLintelChecker : IChecker {
        private readonly RevitRepository _revitRepository;

        public GeometricalLintelChecker(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            var lintelLocationPoint = ((LocationPoint) lintel.Location).Point;
            var elementInWallPoint = _revitRepository.GetLocationPoint(elementInWall);

            var elementWidth = elementInWall.GetParamValueOrDefault("ADSK_Размер_Ширина") ?? 
                               elementInWall.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM); //ToDo: параметр

            return lintelLocationPoint.DistanceTo(elementInWallPoint) < (double) elementWidth / 2 ? null : new LintelGeometricalDisplaced(lintel.Id);
            //Todo: возможно, расстояние должно быть меньше
        }
    }
}
