using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class ClashIsolationViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IClashViewModel _clashModel;
    private readonly SettingsConfig _config;

    public ClashIsolationViewSettings(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IClashViewModel clashModel,
        SettingsConfig config) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public void Apply(View3D view3D) {
        var bboxSettings = new BboxViewSettings(_revitRepository, _clashModel.GetElements(), 10);
        bboxSettings.Apply(view3D);
        IsolateClashElements(view3D, _clashModel);
        var colorSettings = new ColorClashViewSettings(_revitRepository, _localizationService, _clashModel, _config);
        colorSettings.Apply(view3D);
    }

    private void IsolateClashElements(View3D view, IClashViewModel clash) {
        using(Transaction t = _revitRepository.Doc.StartTransaction(
            _localizationService.GetLocalizedString("Transactions.IsolateClashElements"))) {
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

    private ICollection<ParameterFilterElement> GetIsolationFilters(IClashViewModel clash, View view) {

        string username = _revitRepository.Doc.Application.Username;
        var filters = new List<ParameterFilterElement>() {
                _revitRepository.ParameterFilterProvider.GetExceptCategoriesFilter(
                    _revitRepository.Doc,
                    view,
                    GetCategories(clash.GetElements()),
                    _localizationService.GetLocalizedString("Filters.NotCollisionCategories", username))
            };
        var firstEl = GetFirstElement(clash)?.GetElement(_revitRepository.DocInfos);
        var secondEl = GetSecondElement(clash)?.GetElement(_revitRepository.DocInfos);
        if(firstEl?.Category.GetBuiltInCategory() == secondEl?.Category.GetBuiltInCategory()) {
            filters.Add(
                _revitRepository.ParameterFilterProvider.GetHighlightFilter(
                    _revitRepository.Doc,
                    firstEl,
                    secondEl,
                    _localizationService.GetLocalizedString("Filters.NotElementsOfCollisionCategories", username)));
        } else {
            if(firstEl != null) {
                filters.Add(
                    _revitRepository.ParameterFilterProvider.GetHighlightFilter(
                        _revitRepository.Doc,
                        firstEl,
                        _localizationService.GetLocalizedString("Filters.NotFirstElementFilter", username)));
            }
            if(secondEl != null) {
                filters.Add(
                    _revitRepository.ParameterFilterProvider.GetHighlightFilter(
                        _revitRepository.Doc,
                        secondEl,
                        _localizationService.GetLocalizedString("Filters.NotSecondElementFilter", username)));
            }
        }
        return filters;
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

    private ICollection<BuiltInCategory> GetCategories(ICollection<ElementModel> elements) {
        return elements.Select(e => e.GetElement(_revitRepository.DocInfos))
            .Where(e => e != null)
            .Select(e => e.Category.GetBuiltInCategory())
            .ToHashSet();
    }
}
