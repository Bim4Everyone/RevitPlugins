using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.ViewModels.Filtration;
using RevitSleeves.Views.Settings;

using DocInfo = RevitClashDetective.Models.DocInfo;

namespace RevitSleeves.Services.Settings;
internal class FilterChecker : IFilterChecker {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly RevitRepository _revitRepository;
    private readonly SleevePlacementSettingsConfig _config;

    public FilterChecker(IResolutionRoot resolutionRoot, RevitRepository revitRepository,
        SleevePlacementSettingsConfig config) {
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public void ShowFilter(ILogicalFilterContext filterContext) {
        var searchTargets = ContainsMepCategories(filterContext.SelectedCategories)
            ? GetActiveDocSearchTargets()
            : GetStructureLinksSearchTargets();

        var vm = _resolutionRoot.Get<FilterViewModel>(
            new Ninject.Parameters.ConstructorArgument("filterContext", filterContext),
            new Ninject.Parameters.ConstructorArgument("searchTargets", searchTargets));

        var window = _resolutionRoot.Get<FilterWindow>();
        window.DataContext = vm;
        window.Show();
    }

    private bool ContainsMepCategories(ICollection<BuiltInCategory> categories) {
        return categories.Contains(_config.PipeSettings.Category);
    }

    private ICollection<DocInfo> GetActiveDocSearchTargets() {
        return [new DocInfo(
            RevitClashDetective.Models.RevitRepository.GetDocumentName(_revitRepository.Document),
            _revitRepository.Document,
            Transform.Identity)];
    }

    private ICollection<DocInfo> GetStructureLinksSearchTargets() {
        return [.. _revitRepository.GetStructureLinkInstances()
            .Select(link => new DocInfo(
                RevitClashDetective.Models.RevitRepository.GetDocumentName(link.GetLinkDocument()),
                link.GetLinkDocument(),
                link.GetTransform()))];
    }
}
