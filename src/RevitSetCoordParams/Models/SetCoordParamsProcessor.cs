using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Settings;


namespace RevitSetCoordParams.Models;
internal class SetCoordParamsProcessor {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly SetCoordParamsSettings _settings;

    public SetCoordParamsProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        SetCoordParamsSettings settings) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    /// <summary>
    /// Основной метод поиска пересечений и заполнения параметров    
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и элементов основного файла.    
    /// При успешном пересечении записываются параметры из объемного элемента в элемент модели
    /// </remarks>
    /// <returns>Возвращает коллекцию предупреждений WarningModel</returns>
    public IReadOnlyCollection<WarningModel> Run() {
        var sourceModels = _settings.FileProvider.GetRevitElements(_settings.TypeModel);
        var targetElements = _settings.ElementsProvider.GetRevitElements(_settings.Categories);
        var positionProvider = _settings.PositionProvider;
        var sphereProvider = _settings.SphereProvider;
        double startDiam = UnitUtils.ConvertToInternalUnits(RevitConstants.StartDiameterSearchSphereMm, UnitTypeId.Millimeters);
        double maxDiam = UnitUtils.ConvertToInternalUnits(_settings.MaxDiameterSearchSphereMm, UnitTypeId.Millimeters);
        double stepDiam = UnitUtils.ConvertToInternalUnits(_settings.StepDiameterSearchSphereMm, UnitTypeId.Millimeters);

        var allParamMaps = _settings.ParamMaps;
        var blockingParam = allParamMaps
            .FirstOrDefault(paramMap => paramMap.Type == ParamType.BlockingParam);

        var pairParamMaps = allParamMaps
            .Where(paramMap => paramMap.Type != ParamType.BlockingParam);

        string transactionName = _localizationService.GetLocalizedString("SetCoordParamsProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        List<WarningModel> warnings = [];
        foreach(var targetElement in targetElements) {
            if(blockingParam != null) {
                if(targetElement.Element.IsExistsParam(blockingParam.TargetParam.Name)) {
                    int blockValue = targetElement.Element.GetParamValueOrDefault<int>(blockingParam.TargetParam.Name);
                    if(blockValue == 1) {
                        warnings.Add(new WarningBlockElement {
                            WarningDescription = WarningDescription.Blocked,
                            Element = targetElement.Element,
                            Caption = ""
                        });
                        continue;
                    }
                }
            }

            var position = positionProvider.GetPositionElement(targetElement);
            var sphere = sphereProvider.GetSphere(position, startDiam);

            var intersector = new Intersector();
            var sourceModel = intersector.Intersect(sphere, sourceModels);

            double currentDiam = startDiam;
            if(!intersector.HasIntersection && _settings.Search) {

                while(currentDiam < maxDiam) {
                    currentDiam += stepDiam;
                    sphere = sphereProvider.GetSphere(position, currentDiam);
                    sourceModel = intersector.Intersect(sphere, sourceModels);

                    if(intersector.HasIntersection) {
                        break;
                    }
                }
            }


            if(intersector.HasIntersection && sourceModel != null) {
                foreach(var paramMap in pairParamMaps) {

                    if(targetElement.Element.IsExistsParam(paramMap.TargetParam.Name)
                        && sourceModel.Element.IsExistsParam(paramMap.SourceParam.Name)) {

                        if(paramMap.Type == ParamType.FloorDEParam) {
                            double value = sourceModel.Element.GetParamValueOrDefault<double>(paramMap.SourceParam.Name);
                            targetElement.Element.SetParamValue(paramMap.TargetParam.Name, value);
                        } else {
                            string value = sourceModel.Element.GetParamValueOrDefault<string>(paramMap.SourceParam.Name);
                            targetElement.Element.SetParamValue(paramMap.TargetParam.Name, value);
                        }

                    } else {
                        warnings.Add(new WarningParamModel {
                            WarningDescription = WarningDescription.NotFoundParam,
                            Element = targetElement.Element,
                            RevitParam = paramMap.TargetParam,
                            Caption = ""
                        });
                    }
                }
            } else {
                warnings.Add(new WarningSkipElement {
                    WarningDescription = WarningDescription.NotFound,
                    Element = targetElement.Element,
                    Caption = ""
                });
            }
        }
        warnings.Add(new WarningSkipElement {
            WarningDescription = WarningDescription.NotFound,
            Caption = ""
        });
        t.Commit();
        return warnings;
    }
}
