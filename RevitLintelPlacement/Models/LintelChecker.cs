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

        public LintelChecker(RevitRepository revitRepository, GroupedRuleCollectionViewModel groupedRules, ElementInfosViewModel elementInfos) {
            _checkers = new List<IChecker> {
            new LintelGroupChecker(),
            new ElementInWallChecker(revitRepository, elementInfos),
            new LintelWallAboveChecker(revitRepository),
            new LintelRuleChecker(revitRepository, groupedRules, elementInfos),
            new GeometricalLintelChecker(revitRepository, elementInfos)
            };
        }

        public void Check(IEnumerable<LintelInfoViewModel> lintels) {
            foreach(var lintel in lintels) {
                foreach(var checker in _checkers) {
                    var resultHandler = checker.Check(lintel.Lintel, lintel.ElementInWall); 
                    resultHandler?.Handle();
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
        private readonly ElementInfosViewModel _elementInfos;

        public ElementInWallChecker(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(elementInWall != null) {
                return new EmptyResult { Code = ResultCode.Correct };
            }

            if((int) lintel.GetParamValue(_revitRepository.LintelsCommonConfig.LintelFixation) == 1) {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(lintel.Id, InfoElement.LintelIsFixedWithoutElement));
                return new ReportResult(lintel.Id) { Code = ResultCode.LintelIsFixedWithoutElement};
            }
            return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.LintelWithoutElement };
        }
    }

    internal class LintelWallAboveChecker : IChecker {
        private readonly View3D _view3D;
        private readonly RevitRepository _revitRepository;
        private List<string> _links;

        public LintelWallAboveChecker(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            _view3D = _revitRepository.GetView3D();
            List<string> links = _revitRepository.GetLinkTypes().Select(l=>l.Name).ToList();
        }
        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(_revitRepository.CheckUp(_view3D, elementInWall, _links))
                return new EmptyResult() { Code = ResultCode.Correct };
            else
                return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.LintelWithWrongWallAbove };
        }
    }

    internal class LintelRuleChecker : IChecker {
        private readonly View3D _view3D;
        private readonly GroupedRuleCollectionViewModel _groupedRules;
        private readonly RevitRepository _revitRepository;

        public LintelRuleChecker(RevitRepository revitRepository, GroupedRuleCollectionViewModel groupedRules, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._groupedRules = groupedRules;
            _view3D = _revitRepository.GetView3D();
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            var rule = _groupedRules.GetRule(elementInWall);
            if(rule == null) {
                if ((int) lintel.GetParamValue("Фиксация Решения") == 1) {
                    return new EmptyResult { Code = ResultCode.Correct };
                } else {
                    return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.ElementInWallWithoutRule };
                }
                    
            } else {
                var results = new List<ParameterCheckResult> { CheckLintelType(lintel, rule) };
                return new WrongLintelParameters(_revitRepository, results, rule, lintel, elementInWall);
                //проверка корректности параметров перемычки
            }
            return new EmptyResult() { Code = ResultCode.Correct };
        }

        private ParameterCheckResult CheckWallThickness(FamilyInstance lintel, FamilyInstance elementInWall) {
            var wall = elementInWall.Host as Wall;
            var wallThickness = wall.Width;
            var lintelThickness = ((double) lintel.GetParamValue(_revitRepository.LintelsCommonConfig.LintelThickness));
            if(Math.Abs(lintelThickness - wallThickness) < 0.1) {
                return ParameterCheckResult.Correct;
            }
            return ParameterCheckResult.WrongLintelThickness;
        }

        private ParameterCheckResult CheckLintelType(FamilyInstance lintel, ConcreteRuleViewModel rule) {
            if (lintel.Symbol.Name.Equals(rule.SelectedLintelType, StringComparison.CurrentCultureIgnoreCase)) {
                return ParameterCheckResult.Correct;
            }
            return ParameterCheckResult.WrongLintelType;
        }

    }

    internal class GeometricalLintelChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;

        public GeometricalLintelChecker(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            var lintelLocationPoint = ((LocationPoint) lintel.Location).Point;
            var elementInWallPoint = _revitRepository.GetLocationPoint(elementInWall);

            if(lintelLocationPoint.DistanceTo(elementInWallPoint) < 1) //ToDO: понять, что считать смещенной перемычкой - пока примерно 30 см
                return new EmptyResult { Code = ResultCode.Correct };
            else {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(lintel.Id, InfoElement.LintelGeometricalDisplaced));
                return new ReportResult(lintel.Id) { Code = ResultCode.LintelGeometricalDisplaced };
            }
        }
    }
}
