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

        public LintelChecker(RevitRepository revitRepository, GroupedRuleCollectionViewModel groupedRules, IEnumerable<string> links, ElementInfosViewModel elementInfos) {
            _checkers = new List<IChecker> {
            new LintelGroupChecker(),
            new ElementInWallChecker(revitRepository, elementInfos),
            new LintelWallAboveChecker(revitRepository),
            new LintelRuleChecker(revitRepository, groupedRules, links, elementInfos),
            new GeometricalLintelChecker(revitRepository, elementInfos)
            };
        }

        public void Check(ICollection<LintelInfoViewModel> lintels) {
            foreach(var lintel in lintels) {
                foreach(var checker in _checkers) {
                    var resultHandler = checker.Check(lintel.Lintel, lintel.ElementInWall);
                    resultHandler.Handle();
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
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(lintel.Id, InfoElement.LintelIsFixedWithoutElement) {
                    Name = lintel.Name,
                    LevelName = lintel.LevelId != null ? _revitRepository.GetElementById(lintel.LevelId).Name : null
                });
                return new ReportResult(lintel.Id) { Code = ResultCode.LintelIsFixedWithoutElement };
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
            _links = _revitRepository.GetLinkTypes().Select(l => l.Name).ToList();
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
        private readonly ElementInfosViewModel _elementInfos;
        private readonly RevitRepository _revitRepository;
        private readonly List<string> _linkNames;

        public LintelRuleChecker(RevitRepository revitRepository, GroupedRuleCollectionViewModel groupedRules, IEnumerable<string> links, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._groupedRules = groupedRules;
            this._elementInfos = elementInfos;
            _view3D = _revitRepository.GetView3D();
            _linkNames = links.ToList();
        }

        public IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall) {
            var rule = _groupedRules.GetRule(elementInWall);
            if((int) lintel.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.LintelFixation, 0) == 1) {
                return new EmptyResult { Code = ResultCode.Correct };
            }
            if(rule == null) {
                return new LintelForDeletionResult(_revitRepository, lintel) { Code = ResultCode.ElementInWallWithoutRule };
            } else {
                var results = new List<ParameterCheckResult> {
                    CheckLintelType(lintel, rule),
                    CheckLintelThickness(lintel, elementInWall),
                    CheckLintelWidth(lintel, elementInWall),
                    CheckLintelLeftOffset(lintel, elementInWall, rule, out double leftOffset),
                    CheckLintelRightOffset(lintel, elementInWall, rule, out double rightOffset),
                };
                return new WrongLintelParameters(_revitRepository, results, rule, lintel, elementInWall, rightOffset, leftOffset) { Code = ResultCode.Correct };
            }
        }

        private ParameterCheckResult CheckLintelType(FamilyInstance lintel, ConcreteRuleViewModel rule) {
            if(lintel.Symbol.Name.Equals(rule.SelectedLintelType, StringComparison.CurrentCultureIgnoreCase)) {
                return ParameterCheckResult.Correct;
            }
            return ParameterCheckResult.WrongLintelType;
        }

        private ParameterCheckResult CheckLintelThickness(FamilyInstance lintel, FamilyInstance elementInWall) {
            var wall = elementInWall.Host as Wall;
            var wallThickness = wall.Width;
            var lintelThickness = (double) lintel.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.LintelThickness);
            if(Math.Abs(wallThickness - lintelThickness) < 0.01) {
                return ParameterCheckResult.Correct;
            } else {
                return ParameterCheckResult.WrongLintelThickness;
            }
        }

        private ParameterCheckResult CheckLintelWidth(FamilyInstance lintel, FamilyInstance elementInWall) {
            var elementInWallWidth =
                elementInWall.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningWidth)
                ?? elementInWall.Symbol.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningWidth);
            
            if(elementInWallWidth == null) {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                   InfoElement.MissingOpeningParameter.FormatMessage(_revitRepository.LintelsCommonConfig.OpeningWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null ? _revitRepository.GetElementById(elementInWall.LevelId)?.Name : null
                });
                return ParameterCheckResult.Correct;
            }
            
            var lintelWidth = (double) lintel.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.LintelWidth);
            if(Math.Abs((double) elementInWallWidth - lintelWidth) < 0.01) {
                return ParameterCheckResult.Correct;
            } else {
                return ParameterCheckResult.WrongLintelWidth;
            }
        }

        private ParameterCheckResult CheckLintelLeftOffset(FamilyInstance lintel, FamilyInstance elementInWall, ConcreteRuleViewModel rule, out double offset) {
            var lintelLeftOffset = lintel.GetParamValueOrDefault<double>(_revitRepository.LintelsCommonConfig.LintelLeftOffset);
            var lintelCorner = lintel.GetParamValueOrDefault<int>(_revitRepository.LintelsCommonConfig.LintelLeftCorner) == 1 ? true : false;
            if(_revitRepository.DoesLeftCornerNeeded(_view3D, elementInWall, _linkNames, _elementInfos, rule.LintelRightOffsetParameter.RightOffsetInternal, out offset)) {
                if(Math.Abs(lintelLeftOffset - offset) < 0.01 && lintelCorner) {
                    return ParameterCheckResult.Correct;
                } else {
                    return ParameterCheckResult.HasNotLintelLeftCorner;
                }
            } else {
                if(lintelCorner) {
                    return ParameterCheckResult.HasLintelLeftCorner;
                }
                if(Math.Abs(lintelLeftOffset - rule.LintelLeftOffsetParameter.LeftOffset) < 0.01) {
                    return ParameterCheckResult.Correct;
                }
                return ParameterCheckResult.WrongLintelLeftOffset;
            }
        }

        private ParameterCheckResult CheckLintelRightOffset(FamilyInstance lintel, FamilyInstance elementInWall, ConcreteRuleViewModel rule, out double offset) {
            var lintelRightOffset = lintel.GetParamValueOrDefault<double>(_revitRepository.LintelsCommonConfig.LintelRightOffset);
            var lintelCorner = lintel.GetParamValueOrDefault<int>(_revitRepository.LintelsCommonConfig.LintelRightCorner) == 1 ? true : false;

            if(_revitRepository.DoesRightCornerNeeded(_view3D, elementInWall, _linkNames, _elementInfos, rule.LintelRightOffsetParameter.RightOffsetInternal, out offset)) {
                if(Math.Abs(lintelRightOffset - offset) < 0.01 && lintelCorner) {
                    return ParameterCheckResult.Correct;
                } else {
                    return ParameterCheckResult.HasNotLintelRightCorner;
                }
            } else {
                if(lintelCorner) {
                    return ParameterCheckResult.HasLintelRightCorner;
                }
                if(Math.Abs(lintelRightOffset - rule.LintelRightOffsetParameter.RightOffset) < 0.01) {
                    return ParameterCheckResult.Correct;
                }
                return ParameterCheckResult.WrongLintelRightOffset;
            }
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
            double height = lintel.LevelId.IsNotNull() ? ((Level) _revitRepository.GetElementById(lintel.LevelId)).Elevation : 0;
            var elementInWallPoint = _revitRepository.GetLocationPoint(elementInWall) + XYZ.BasisZ * height;

            if(lintelLocationPoint.DistanceTo(elementInWallPoint) < 0.0001)
                return new EmptyResult { Code = ResultCode.Correct };
            else {
                _elementInfos.ElementInfos.Add(new ElementInfoViewModel(lintel.Id, InfoElement.LintelGeometricalDisplaced) {
                    Name = lintel.Name,
                    LevelName = lintel.LevelId != null ? _revitRepository.GetElementById(lintel.LevelId)?.Name : null
                });
                return new ReportResult(lintel.Id) { Code = ResultCode.LintelGeometricalDisplaced };
            }
        }
    }
}
