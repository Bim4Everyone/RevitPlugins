using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using Ninject;
using Ninject.Syntax;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.ReportElements;
using RevitMarkPlacement.ViewModels.ReportViewModels;
using RevitMarkPlacement.Views;

namespace RevitMarkPlacement.Services;

internal sealed class ReportService : IReportService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ILocalizationService _localizationService;

    private readonly ReportDescription _missingParamDescription;
    private readonly ReportDescription _missingFamilyDescription;

    private readonly List<ReportElement> _reportElements = [];

    public ReportService(
        IResolutionRoot resolutionRoot,
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig,
        ILocalizationService localizationService) {
        _resolutionRoot = resolutionRoot;
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _localizationService = localizationService;

        _missingParamDescription = new ReportDescription() {
            Id = "PARAM001",
            ReportLevel = ReportLevel.Warning,
            Title = _localizationService.GetLocalizedString("Report.MissingParamTitle"),
            Description = _localizationService.GetLocalizedString("Report.MissingParamDescription"),
            MessageFormat = _localizationService.GetLocalizedString("Report.MissingParamMessageFormat"),
        };

        _missingFamilyDescription = new ReportDescription() {
            Id = "FAMILY001",
            ReportLevel = ReportLevel.Warning,
            Title = _localizationService.GetLocalizedString("Report.MissingFamilyTitle"),
            Description = _localizationService.GetLocalizedString("Report.MissingFamilyDescription"),
            MessageFormat = _localizationService.GetLocalizedString("Report.MissingFamilyMessageFormat"),
        };
    }

    public bool LoadReportElements() {
        _reportElements.Clear();

        var families = GetFamilies();
        foreach(Family family in families) {
            using var familyDocument = _revitRepository.LoadFamilyDocument(family);

            var paramNames = familyDocument.FamilyManager.GetParameters()
                .Select(item => item.Definition.Name);

            var missedParams = _systemPluginConfig.FamilyParamsNames
                .Except(paramNames);

            foreach(string paramName in missedParams) {
                _reportElements.Add(new ReportElement(_missingParamDescription, family.Name, paramName));
            }
        }

        return _reportElements.Count > 0;
    }

    public void ShowReport() {
        var reportElementsWindow = _resolutionRoot.Get<ReportElementsWindow>();
        var reportElementsViewModel = (ReportElementsViewModel) reportElementsWindow.DataContext;

        reportElementsViewModel.ReportElements.Clear();
        foreach(var group in _reportElements.GroupBy(item=> item.Description)) {
            var report = group.Key;
            var reportMessages = group.Select(item => item.FormattedMessage);
            reportElementsViewModel.ReportElements.Add(new ReportElementViewModel(report, reportMessages));
        }

        if(reportElementsViewModel.ReportElements.Count == 0) {
            return;
        }

        reportElementsWindow.ShowDialog();
    }

    private IEnumerable<Family> GetFamilies() {
        var topAnnotationFamily = _revitRepository.GetAnnotationFamily(_systemPluginConfig.FamilyTopName);
        if(topAnnotationFamily is not null) {
            yield return topAnnotationFamily;
        } else {
            _reportElements.Add(new ReportElement(_missingFamilyDescription, _systemPluginConfig.FamilyTopName));
        }

        var bottomAnnotationFamily = _revitRepository.GetAnnotationFamily(_systemPluginConfig.FamilyBottomName);
        if(bottomAnnotationFamily is not null) {
            yield return topAnnotationFamily;
        } else {
            _reportElements.Add(new ReportElement(_missingFamilyDescription, _systemPluginConfig.FamilyBottomName));
        }
    }
}
