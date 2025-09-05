using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.RevitViewSettings;
internal class ColorClashViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IClashViewModel _clashModel;
    private readonly SettingsConfig _config;

    public ColorClashViewSettings(RevitRepository revitRepository,
        ILocalizationService localizationService,
        IClashViewModel clashModel,
        SettingsConfig config) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public void Apply(View3D view3D) {
        ColorClashElements(view3D, _clashModel);
    }

    private void ColorClashElements(View3D view, IClashViewModel clash) {
        var firstEl = GetFirstElement(clash)?.GetElement(_revitRepository.DocInfos);
        var secondEl = GetSecondElement(clash)?.GetElement(_revitRepository.DocInfos);
        if(firstEl != null || secondEl != null) {
            string username = _revitRepository.Doc.Application.Username;
            using var t = _revitRepository.Doc.StartTransaction(
                _localizationService.GetLocalizedString("Transactions.ClashGraphicSettings"));
            if(firstEl != null) {
                SetViewColorSettings(view,
                    firstEl,
                    _localizationService.GetLocalizedString("Filters.FirstElementFilter", username),
                    GetGraphicSettings(_config.MainElementVisibilitySettings));
            }
            if(secondEl != null) {
                SetViewColorSettings(view,
                    secondEl,
                    _localizationService.GetLocalizedString("Filters.SecondElementFilter", username),
                    GetGraphicSettings(_config.SecondElementVisibilitySettings));
            }

            t.Commit();
        }
    }

    private void SetViewColorSettings(View3D view,
        Element element,
        string filterName,
        OverrideGraphicSettings graphicSettings) {

        var secondElFilter = _revitRepository.ParameterFilterProvider.GetSelectFilter(
            _revitRepository.Doc, element, filterName);
        view.AddFilter(secondElFilter.Id);
        view.SetFilterOverrides(secondElFilter.Id, graphicSettings);
    }

    private ElementModel GetFirstElement(IClashViewModel clash) {
        try {
            return clash.GetFirstElement();
        } catch(NotSupportedException) {
            return null;
        }
    }

    private ElementModel GetSecondElement(IClashViewModel clash) {
        try {
            return clash.GetSecondElement();
        } catch(NotSupportedException) {
            return null;
        }
    }

    private OverrideGraphicSettings GetGraphicSettings(ElementVisibilitySettings settings) {
        var color = new Color(settings.Color.R, settings.Color.G, settings.Color.B);
        var graphicOverrides = new OverrideGraphicSettings();

        graphicOverrides
            .SetSurfaceTransparency(settings.Transparency)
            .SetCutBackgroundPatternColor(color)
            .SetCutForegroundPatternColor(color)
            .SetSurfaceBackgroundPatternColor(color)
            .SetSurfaceForegroundPatternColor(color);

        var solidFillPattern = new FilteredElementCollector(_revitRepository.Doc)
            .OfClass(typeof(FillPatternElement))
            .OfType<FillPatternElement>()
            .FirstOrDefault(item => item.GetFillPattern().IsSolidFill);
        if(solidFillPattern != null) {
            graphicOverrides
                .SetSurfaceBackgroundPatternId(solidFillPattern.Id)
                .SetSurfaceForegroundPatternId(solidFillPattern.Id)
                .SetCutBackgroundPatternId(solidFillPattern.Id)
                .SetCutForegroundPatternId(solidFillPattern.Id);
        }
        return graphicOverrides;
    }
}
