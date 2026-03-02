using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Settings;


namespace RevitSetCoordParams.Models;
internal class IntersectCurveProcessor : IIntersectProcessor {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly SetCoordParamsSettings _settings;

    public IntersectCurveProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        SetCoordParamsSettings settings) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    public IEnumerable<RevitElement> RevitElements => GetRevitElements();

    public IReadOnlyCollection<WarningElement> Run(IProgress<int> progress = null, CancellationToken ct = default) {
        var sourceModels = _settings.TypeModels
            .SelectMany(_settings.FileProvider.GetRevitElements)
            .ToArray();
        var targetElements = RevitElements;
        var positionProvider = _settings.PositionProvider;
        double startDiam = UnitUtils.ConvertToInternalUnits(RevitConstants.StartDiameterSearchSphereMm, UnitTypeId.Millimeters);
        double maxDiam = UnitUtils.ConvertToInternalUnits(_settings.MaxDiameterSearchSphereMm, UnitTypeId.Millimeters);
        double stepDiam = UnitUtils.ConvertToInternalUnits(_settings.StepDiameterSearchSphereMm, UnitTypeId.Millimeters);

        var allParamMaps = _settings.ParamMaps;
        var blockingParamMap = allParamMaps
            .FirstOrDefault(paramMap => paramMap.Type == ParamType.BlockingParam);
        var pairParamMaps = allParamMaps
            .Where(paramMap => paramMap.Type != ParamType.BlockingParam);

        double intersectCurveLength = UnitUtils.ConvertToInternalUnits(RevitConstants.IntersectCurveLengthMm, UnitTypeId.Millimeters);
        var offsetIntersectLine = new XYZ(0, 0, intersectCurveLength);

        var intersector = new CurveIntersector(sourceModels);

        string transactionName = _localizationService.GetLocalizedString("SetCoordParamsProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        int i = 0;
        List<WarningElement> warnings = [];
        foreach(var targetElement in targetElements) {
            ct.ThrowIfCancellationRequested();

            if(IsBlocked(blockingParamMap, targetElement)) {
                warnings.Add(new WarningSkipElement {
                    WarningType = WarningType.SkipElement,
                    RevitElement = targetElement
                });
                continue;
            }

            var position = positionProvider.GetPositionElement(targetElement);
            var curve = GetIntersectCurve(position, offsetIntersectLine);

            var sourceModel = intersector.IntersectWithCurve(curve);

            double currentDiam = startDiam;
            if(sourceModel == null && _settings.Search) {
                while(currentDiam < maxDiam) {
                    currentDiam += stepDiam;
                    var curves = GetSphereLine(position, currentDiam);

                    sourceModel = intersector.IntersectWithCurves(curves);

                    if(sourceModel is not null) {
                        break;
                    }
                }
            }

            if(sourceModel is not null) {
                foreach(var paramMap in pairParamMaps) {
                    string targetParamName = paramMap.TargetParam.Name;
                    string sourceParamName = paramMap.SourceParam.Name;

                    if(targetElement.Element.IsExistsParam(targetParamName)
                        && sourceModel.Element.IsExistsParam(sourceParamName)) {
                        if(paramMap.Type == ParamType.FloorDEParam) {
                            double value = sourceModel.Element.GetParamValueOrDefault<double>(sourceParamName);
                            if(value != default) {
                                targetElement.Element.SetParamValue(targetParamName, value);
                            }
                        } else {
                            string value = sourceModel.Element.GetParamValueOrDefault<string>(sourceParamName);
                            if(value != null) {
                                targetElement.Element.SetParamValue(targetParamName, value);
                            }
                        }
                    } else {
                        warnings.Add(new WarningNotFoundParamElement {
                            WarningType = WarningType.NotFoundParameter,
                            RevitElement = targetElement,
                            RevitParam = paramMap.TargetParam
                        });
                    }
                }
            } else {
                warnings.Add(new WarningNotFoundElement {
                    WarningType = WarningType.NotFoundElement,
                    RevitElement = targetElement
                });
            }
            progress?.Report(++i);
        }
        t.Commit();
        return warnings;
    }

    // Метод получения элементов модели для основного метода и прогресс-бара  
    private IEnumerable<RevitElement> GetRevitElements() {
        return _settings.ElementsProvider.GetRevitElements(_settings.Categories);
    }

    private bool IsBlocked(ParamMap blockingParamMap, RevitElement targetElement) {
        if(blockingParamMap != null) {
            string targetParamNane = blockingParamMap.TargetParam.Name;
            if(targetElement.Element.IsExistsParam(targetParamNane)) {
                int blockValue = targetElement.Element.GetParamValueOrDefault<int>(targetParamNane);
                if(blockValue is not default(int) and 1) {
                    return true;
                }
            }
        }
        return false;
    }

    // Метод получения кривой для пересечения с объемным элементом        
    private Curve GetIntersectCurve(XYZ origin, XYZ offset) {
        return Line.CreateBound(origin, origin + offset);
    }

    // Метод получения сферы из линий   
    private List<Curve> GetSphereLine(XYZ origin, double diameter) {
        double r = diameter / 2.0;
        var curves = new List<Curve>(6);
        {
            var top = new XYZ(origin.X, origin.Y, origin.Z + r);
            var bottom = new XYZ(origin.X, origin.Y, origin.Z - r);
            var right = new XYZ(origin.X + r, origin.Y, origin.Z);
            var left = new XYZ(origin.X - r, origin.Y, origin.Z);

            curves.Add(Arc.Create(bottom, top, right));
            curves.Add(Arc.Create(top, bottom, left));
        }
        {
            var top = new XYZ(origin.X, origin.Y, origin.Z + r);
            var bottom = new XYZ(origin.X, origin.Y, origin.Z - r);
            var front = new XYZ(origin.X, origin.Y + r, origin.Z);
            var back = new XYZ(origin.X, origin.Y - r, origin.Z);

            curves.Add(Arc.Create(bottom, top, front));
            curves.Add(Arc.Create(top, bottom, back));
        }
        {
            var right = new XYZ(origin.X + r, origin.Y, origin.Z);
            var left = new XYZ(origin.X - r, origin.Y, origin.Z);
            var front = new XYZ(origin.X, origin.Y + r, origin.Z);
            var back = new XYZ(origin.X, origin.Y - r, origin.Z);

            curves.Add(Arc.Create(left, right, front));
            curves.Add(Arc.Create(right, left, back));
        }

        return curves;
    }
}
