using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class ClashIsolationViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ClashModel _clashModel;
    private readonly SettingsConfig _config;

    public ClashIsolationViewSettings(RevitRepository revitRepository, ClashModel clashModel, SettingsConfig config) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public void Apply(View3D view3D) {
        var bboxSettings = new BboxViewSettings(_revitRepository,
            [_clashModel.MainElement, _clashModel.OtherElement],
            10);
        bboxSettings.Apply(view3D);
        IsolateClashElements(view3D, _clashModel);
        ColorClashElements(view3D, _clashModel);
    }

    private void IsolateClashElements(View3D view, ClashModel clash) {
        using(Transaction t = _revitRepository.Doc.StartTransaction("Изоляция элементов коллизии")) {
            var filtersToHide = GetIsolationFilters(clash, view);
            view = _revitRepository.RemoveFilters(view);
            foreach(var filter in filtersToHide) {
                view.AddFilter(filter.Id);
                view.SetFilterVisibility(filter.Id, false);
            }
            foreach(var category in _revitRepository.GetLineCategoriesToHide()) {
                view.SetCategoryHidden(category, true);
            }
            t.Commit();
        }
    }

    private void ColorClashElements(View3D view, ClashModel clash) {
        var firstEl = clash.MainElement.GetElement(_revitRepository.DocInfos);
        var secondEl = clash.OtherElement.GetElement(_revitRepository.DocInfos);
        if(firstEl != null && secondEl != null) {
            string username = _revitRepository.Doc.Application.Username;
            using(Transaction t = _revitRepository.Doc.StartTransaction("Настройка графики элементов коллизии")) {
                var firstElFilter = _revitRepository.ParameterFilterProvider.GetSelectFilter(
                    _revitRepository.Doc,
                    firstEl,
                    $"{RevitRepository.FiltersNamePrefix}первый_элемент_{username}");
                var secondElFilter = _revitRepository.ParameterFilterProvider.GetSelectFilter(
                    _revitRepository.Doc,
                    secondEl,
                    $"{RevitRepository.FiltersNamePrefix}второй_элемент_{username}");

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

    private ICollection<ParameterFilterElement> GetIsolationFilters(ClashModel clash, View view) {
        string username = _revitRepository.Doc.Application.Username;
        var filters = new List<ParameterFilterElement>() {
                _revitRepository.ParameterFilterProvider.GetExceptCategoriesFilter(
                    _revitRepository.Doc,
                    view,
                    GetClashCategories(clash),
                    $"{RevitRepository.FiltersNamePrefix}не_категории_элементов_коллизии_{username}")
            };
        var firstEl = clash.MainElement.GetElement(_revitRepository.DocInfos);
        var secondEl = clash.OtherElement.GetElement(_revitRepository.DocInfos);
        if(firstEl?.Category.GetBuiltInCategory() == secondEl?.Category.GetBuiltInCategory()) {
            filters.Add(
                _revitRepository.ParameterFilterProvider.GetHighlightFilter(
                    _revitRepository.Doc,
                    firstEl,
                    secondEl,
                    $"{RevitRepository.FiltersNamePrefix}не_элементы_категории_коллизии_{username}"));
        } else {
            if(firstEl != null) {
                filters.Add(
                    _revitRepository.ParameterFilterProvider.GetHighlightFilter(
                        _revitRepository.Doc,
                        firstEl,
                        $"{RevitRepository.FiltersNamePrefix}не_первый_элемент_{username}"));
            }
            if(secondEl != null) {
                filters.Add(
                    _revitRepository.ParameterFilterProvider.GetHighlightFilter(
                        _revitRepository.Doc,
                        secondEl,
                        $"{RevitRepository.FiltersNamePrefix}не_второй_элемент_{username}"));
            }
        }
        return filters;
    }

    private ICollection<BuiltInCategory> GetClashCategories(ClashModel clash) {
        var elements = new[] { clash.MainElement, clash.OtherElement };
        return elements.Select(e => e.GetElement(_revitRepository.DocInfos))
            .Where(e => e != null)
            .Select(e => e.Category.GetBuiltInCategory())
            .ToHashSet();
    }
}
