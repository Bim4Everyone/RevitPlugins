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
internal class IntersectSolidProcessor : IIntersectProcessor {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly SetCoordParamsSettings _settings;

    public IntersectSolidProcessor(
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

        var intersector = new SolidIntersector(sourceModels);

        string transactionName = _localizationService.GetLocalizedString("SetCoordParamsProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        List<WarningElement> warnings = [];
        int i = 0;
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
            var sphere = GetSphereSolid(position, startDiam);
            var sourceModel = intersector.Intersect(sphere);

            double currentDiam = startDiam;
            if(sourceModel == null && _settings.Search) {
                while(currentDiam < maxDiam) {
                    currentDiam += stepDiam;
                    sphere = GetSphereSolid(position, startDiam);
                    sourceModel = intersector.Intersect(sphere);
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
                            targetElement.Element.SetParamValue(targetParamName, value);
                        } else {
                            string value = sourceModel.Element.GetParamValueOrDefault<string>(sourceParamName);
                            targetElement.Element.SetParamValue(targetParamName, value);
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

    // Метод проверки элемента на параметр блокировки
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

    // Метод получения сферы-солида    
    private Solid GetSphereSolid(XYZ location, double diameter) {
        var startPoint = new XYZ(location.X, location.Y, location.Z - diameter / 2);
        var midPoint = new XYZ(location.X + diameter / 2, location.Y, location.Z);
        var endPoint = new XYZ(location.X, location.Y, location.Z + diameter / 2);

        var arc = Arc.Create(startPoint, endPoint, midPoint);
        var line = Line.CreateBound(endPoint, startPoint);

        var curve_loop = CurveLoop.Create([arc, line]);

        int startAngle = 0;
        double endAngle = 2 * Math.PI;

        var frame = new Frame { Origin = location };

        return GeometryCreationUtilities.CreateRevolvedGeometry(frame, [curve_loop], startAngle, endAngle);
    }
}
