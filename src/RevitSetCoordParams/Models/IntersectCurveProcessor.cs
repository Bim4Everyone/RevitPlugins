using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Services;
using RevitSetCoordParams.Models.Settings;


namespace RevitSetCoordParams.Models;
internal class IntersectCurveProcessor : IIntersectProcessor {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly SetCoordParamsSettings _settings;
    private readonly CurveIntersector _intersector;
    private readonly ParamManager _paramManager;
    private readonly ElementStatusService _elementStatusService;

    public IntersectCurveProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        SetCoordParamsSettings settings) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
        _intersector = new CurveIntersector(_settings);
        _paramManager = new ParamManager(_settings);
        _elementStatusService = new ElementStatusService(_revitRepository.Document);
    }

    public IEnumerable<RevitElement> RevitElements => GetRevitElements();

    public IReadOnlyCollection<WarningElement> Run(IProgress<int> progress = null, CancellationToken ct = default) {
        string transactionName = _localizationService.GetLocalizedString("SetCoordParamsProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        int counter = 0;
        List<WarningElement> warnings = [];
        foreach(var targetElement in RevitElements) {
            ct.ThrowIfCancellationRequested();

            if(_paramManager.BlockingCheck(targetElement)) {
                CollectBlockingElementWarnings(targetElement, warnings);
                progress?.Report(++counter);
                continue;
            }

            if(_elementStatusService.IsDeletedOrUpdatedInCentral(targetElement)) {
                CollectDeletedOrUpdatedWarnings(targetElement, warnings);
                progress?.Report(++counter);
                continue;
            }

            if(_elementStatusService.IsElementOccupied(targetElement, user => {
                CollectOccupiedWarnings(targetElement, user, warnings);
            })) {
                progress?.Report(++counter);
                continue;
            }

            if(_settings.DependentProcess == DependentProcess.InheritanceParent) {
                ProcessWithInheritance(targetElement, warnings);
            } else {
                ProcessSingleElement(targetElement, warnings);
            }
            progress?.Report(++counter);
        }
        t.Commit();
        return warnings;
    }

    // Метод назначения параметров в режиме "наследования"
    private void ProcessWithInheritance(RevitElement parent, List<WarningElement> warnings) {
        var foundModel = _intersector.Intersect(parent);
        if(foundModel is null) {
            CollectIntersectWarnings(parent, warnings);
            foreach(var depElement in parent.DependentElements ?? Enumerable.Empty<RevitElement>()) {
                CollectIntersectWarnings(depElement, warnings);
            }
            return;
        }
        ProcessParams(foundModel, parent, warnings);
        foreach(var depElement in parent.DependentElements ?? Enumerable.Empty<RevitElement>()) {
            ProcessParams(foundModel, depElement, warnings);
        }
    }

    // Метод назначения параметров в режиме "по геометрическому положению"
    private void ProcessSingleElement(RevitElement element, List<WarningElement> warnings) {
        ProcessIntersect(element, warnings);

        foreach(var dependentElement in element.DependentElements ?? Enumerable.Empty<RevitElement>()) {
            ProcessIntersect(dependentElement, warnings);
        }
    }

    // Метод назначения параметров и сбора предупреждений
    private void ProcessParams(RevitElement source, RevitElement target, List<WarningElement> warnings) {
        _paramManager.SetParams(source, target, param => {
            CollectParamWarnings(target, param, warnings);
        });
    }

    // Метод пересечений и сбора предупреждений
    private void ProcessIntersect(RevitElement element, List<WarningElement> warnings) {
        var foundModel = _intersector.Intersect(element);
        if(foundModel is null) {
            CollectIntersectWarnings(element, warnings);
        } else {
            ProcessParams(foundModel, element, warnings);
        }
    }

    // Метод сбора предупреждений о неудачных пересечениях
    private void CollectIntersectWarnings(RevitElement element, List<WarningElement> warnings) {
        warnings.Add(new WarningNotFoundElement {
            WarningType = WarningType.NotFoundElement,
            RevitElement = element
        });
    }

    // Метод сбора предупреждений о неудачных присвоениях параметров
    private void CollectParamWarnings(RevitElement element, RevitParam param, List<WarningElement> warnings) {
        warnings.Add(new WarningNotFoundParamElement {
            WarningType = WarningType.NotFoundParameter,
            RevitElement = element,
            RevitParam = param
        });
    }

    // Метод сбора предупреждений о заблокированных элементах
    private void CollectBlockingElementWarnings(RevitElement target, List<WarningElement> warnings) {
        warnings.Add(new WarningSkipElement {
            WarningType = WarningType.SkipElement,
            RevitElement = target
        });

        foreach(var dependentElement in target.DependentElements ?? Enumerable.Empty<RevitElement>()) {
            warnings.Add(new WarningSkipElement {
                WarningType = WarningType.SkipElement,
                RevitElement = dependentElement
            });
        }
    }

    // Метод сбора предупреждений об удаленном или обновленном элементе в центральной модели хранилища
    private void CollectDeletedOrUpdatedWarnings(RevitElement target, List<WarningElement> warnings) {
        warnings.Add(new WarningDeletedInCentralElement {
            WarningType = WarningType.DeletedInCentral,
            RevitElement = target
        });

        foreach(var dependentElement in target.DependentElements ?? Enumerable.Empty<RevitElement>()) {
            warnings.Add(new WarningDeletedInCentralElement {
                WarningType = WarningType.DeletedInCentral,
                RevitElement = dependentElement
            });
        }
    }

    // Метод сбора предупреждений о занятом элементе пользователем
    private void CollectOccupiedWarnings(RevitElement element, string user, List<WarningElement> warnings) {
        warnings.Add(new WarningOccupiedElement {
            WarningType = WarningType.OccupiedElement,
            RevitElement = element,
            User = user
        });
        foreach(var dependentElement in element.DependentElements ?? Enumerable.Empty<RevitElement>()) {
            warnings.Add(new WarningOccupiedElement {
                WarningType = WarningType.OccupiedElement,
                RevitElement = dependentElement,
                User = user
            });
        }
    }

    // Метод получения элементов модели для основного метода и прогресс-бара  
    private IEnumerable<RevitElement> GetRevitElements() {
        return _settings.ElementsProvider.GetRevitElements(_settings.Categories);
    }
}
