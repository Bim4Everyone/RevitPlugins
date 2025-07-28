using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;
internal class ColorClashViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ClashModel _clashModel;
    private readonly SettingsConfig _config;

    public ColorClashViewSettings(RevitRepository revitRepository,
        ILocalizationService localizationService,
        ClashModel clashModel,
        SettingsConfig config) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public void Apply(View3D view3D) {
        ColorClashElements(view3D, _clashModel);
    }

    private void ColorClashElements(View3D view, ClashModel clash) {
        var firstEl = clash.MainElement.GetElement(_revitRepository.DocInfos);
        var secondEl = clash.OtherElement.GetElement(_revitRepository.DocInfos);
        if(firstEl != null && secondEl != null) {
            string username = _revitRepository.Doc.Application.Username;
            using(Transaction t = _revitRepository.Doc.StartTransaction(
                _localizationService.GetLocalizedString("Transactions.ClashGraphicSettings"))) {
                var firstElFilter = _revitRepository.ParameterFilterProvider.GetSelectFilter(
                    _revitRepository.Doc,
                    firstEl,
                    string.Format(_localizationService.GetLocalizedString("Filters.FirstElementFilter", username)));
                var secondElFilter = _revitRepository.ParameterFilterProvider.GetSelectFilter(
                    _revitRepository.Doc,
                    secondEl,
                    string.Format(_localizationService.GetLocalizedString("Filters.SecondElementFilter", username)));

                view.AddFilter(firstElFilter.Id);
                view.SetFilterOverrides(firstElFilter.Id, GetGraphicSettings(_config.MainElementVisibilitySettings));
                view.AddFilter(secondElFilter.Id);
                view.SetFilterOverrides(secondElFilter.Id, GetGraphicSettings(_config.SecondElementVisibilitySettings));
                t.Commit();
            }
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
