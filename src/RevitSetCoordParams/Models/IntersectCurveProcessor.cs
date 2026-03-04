using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        var targetElements = RevitElements;

        var intersector = new CurveIntersector(_settings);
        var paramManager = new ParamManager(_settings);

        string transactionName = _localizationService.GetLocalizedString("SetCoordParamsProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        int i = 0;
        List<WarningElement> warnings = [];
        foreach(var targetElement in targetElements) {
            ct.ThrowIfCancellationRequested();

            if(paramManager.BlockingCheck(targetElement)) {
                warnings.Add(new WarningSkipElement {
                    WarningType = WarningType.SkipElement,
                    RevitElement = targetElement
                });
                progress?.Report(++i);
                continue;
            }

            var foundModel = intersector.Intersect(targetElement);

            if(foundModel is null) {
                warnings.Add(new WarningNotFoundElement {
                    WarningType = WarningType.NotFoundElement,
                    RevitElement = targetElement
                });
                progress?.Report(++i);
                continue;
            }

            var missedParams = paramManager.SetParams(foundModel, targetElement);

            if(!missedParams.Any()) {
                progress?.Report(++i);
                continue;
            }

            foreach(var param in missedParams) {
                warnings.Add(new WarningNotFoundParamElement {
                    WarningType = WarningType.NotFoundParameter,
                    RevitElement = targetElement,
                    RevitParam = param
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
}
