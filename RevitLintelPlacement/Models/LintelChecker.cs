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
            new LintelWallAboveChecker(revitRepository),
            new LintelRuleChecker(revitRepository, rulesSettings),
            new GeometricalLintelChecker(revitRepository)
            };
        }

        public IEnumerable<IResultHandler> Check(IEnumerable<LintelInfoViewModel> lintels) {
            foreach(var lintel in lintels) {
                foreach(var checker in _checkers) {
                    var resultHandler = checker.Check(lintel.Lintel, lintel.ElementInWall); 
                    resultHandler?.Handle();
                    if(resultHandler.Code == ResultCode.LintelGeometricalDisplaced || resultHandler.Code == ResultCode.LintelIsFixedWithoutElement)
                        yield return resultHandler;
                    if(resultHandler.Code != ResultCode.Correct)
                        break;
                }
            }
        }
    }

    internal class LintelGroupChecker : IChecker {
        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel.GroupId != ElementId.InvalidElementId || lintel.SuperComponent != null)
                return new EmptyResult() { Code = ResultCode.LintelInGroup };
            return new EmptyResult() { Code = ResultCode.Correct };
        }
    }

    internal class ElementInWallChecker : IChecker {
        private readonly RevitRepository _revitRepository;

        public ElementInWallChecker(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(elementInWall != null) {
                return new EmptyResult { Code = ResultCode.Correct };
            }

            if((int) lintel.GetParamValue("Фиксировать") == 1) { //Todo: параметр "Фиксировать"
                return new ReportResult(lintel.Id) { Code = ResultCode.LintelIsFixedWithoutElement};
            }
            return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.LintelWithoutElement };
        }
    }

    internal class LintelWallAboveChecker : IChecker {
        private readonly View3D _view3D;
        private readonly RevitRepository _revitRepository;

        public LintelWallAboveChecker(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            _view3D = _revitRepository.GetView3D();
        }
        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(_revitRepository.CheckUp(_view3D, elementInWall))
                return new EmptyResult() { Code = ResultCode.Correct };
            else
                return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.LintelWithWrongWallAbove };
        }
    }

    internal class LintelRuleChecker : IChecker {
        private readonly View3D _view3D;
        private readonly GroupedRuleCollectionViewModel _groupedRules;
        private readonly RevitRepository _revitRepository;

        public LintelRuleChecker(RevitRepository revitRepository, RulesSettings rulesSettings) {
            this._revitRepository = revitRepository;
            this._groupedRules = new GroupedRuleCollectionViewModel(_revitRepository, rulesSettings);
            _view3D = _revitRepository.GetView3D();
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            var rule = _groupedRules.GetRule(elementInWall);
            if(rule == null) {
                if ((int) lintel.GetParamValue("Фиксировать") == 1) {
                    return new EmptyResult { Code = ResultCode.Correct };
                } else {
                    return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.ElementInWallWithoutRule };
                }
                    
            } else {
                //проверка корректности параметров перемычки
            }
            return new EmptyResult() { Code = ResultCode.Correct };
        }

        private ParameterCheckResult CheckWallThickness(FamilyInstance lintel, FamilyInstance elementInWall) {
            var wall = elementInWall.Host as Wall;
            var wallThickness = wall.Width;
            var lintelThickness = ((double) lintel.GetParamValue("Половина толщины стены")) * 2; //ToDo: параметр
            if(Math.Abs(lintelThickness - wallThickness) < 0.1) {
                return ParameterCheckResult.Correct;
            }
            return ParameterCheckResult.WrongLintelThickness;
        }

        //private ParameterCheckResult CheckOpeningWidth(FamilyInstance lintel, FamilyInstance elementInWall) {
            
        //}
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

            if(lintelLocationPoint.DistanceTo(elementInWallPoint) < (double) elementWidth / 2) //Todo: возможно, расстояние должно быть меньше
                return new EmptyResult { Code = ResultCode.Correct }; 
            else
                return new ReportResult(lintel.Id) { Code = ResultCode.LintelGeometricalDisplaced };
        }
    }
}
