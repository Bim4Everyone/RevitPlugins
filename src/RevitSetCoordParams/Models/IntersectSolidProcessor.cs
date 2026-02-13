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

    /// <summary>
    /// Свойство для определения количества элементов модели в прогресс-баре   
    /// </summary>  
    public IEnumerable<RevitElement> RevitElements => GetRevitElements();

    /// <summary>
    /// Основной метод поиска пересечений и заполнения параметров    
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и элементов основного файла.    
    /// При успешном пересечении записываются параметры из объемного элемента в элемент модели
    /// </remarks>
    /// <returns>Возвращает коллекцию предупреждений WarningModel</returns>
    public IReadOnlyCollection<WarningElement> Run(IProgress<int> progress = null, CancellationToken ct = default) {
        var sourceModels = _settings.TypeModels
            .SelectMany(_settings.FileProvider.GetRevitElements).ToArray();
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
            var sphere = _revitRepository.GetSphereSolid(position, startDiam);

            var sourceModel = intersector.Intersect(sphere);

            double currentDiam = startDiam;
            if(sourceModel == null && _settings.Search) {
                while(currentDiam < maxDiam) {
                    currentDiam += stepDiam;
                    sphere = _revitRepository.GetSphereSolid(position, startDiam);

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

    // Метод получения элементов модели для основного метода и прогресс-бара  
    private IEnumerable<RevitElement> GetRevitElements() {
        return _settings.ElementsProvider.GetRevitElements(_settings.Categories);
    }
}
