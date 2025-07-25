using System.Collections.Generic;

using Autodesk.Revit.DB;

using Ninject;
using Ninject.Syntax;

using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models.Config;
using RevitSleeves.ViewModels.Filtration;
using RevitSleeves.Views.Settings;

namespace RevitSleeves.Services.Settings;
internal class FilterChecker : IFilterChecker {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly SleevePlacementSettingsConfig _config;

    public FilterChecker(IResolutionRoot resolutionRoot, SleevePlacementSettingsConfig config) {
        _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
        _config = config ?? throw new System.ArgumentNullException(nameof(config));
    }


    public void ShowFilter(Filter filter) {
        FilterViewModel vm;
        var filterParameter = new Ninject.Parameters.ConstructorArgument("filter", filter);
        if(ContainsMepCategories(filter.CategoryIds)) {
            vm = _resolutionRoot.Get<ActiveDocFilterViewModel>(filterParameter);
        } else {
            vm = _resolutionRoot.Get<StructureLinksFilterViewModel>(filterParameter);
        }
        var window = _resolutionRoot.Get<FilterWindow>();
        window.DataContext = vm;
        window.Show();
    }

    private bool ContainsMepCategories(ICollection<ElementId> categories) {
        return categories.Contains(new ElementId(_config.PipeSettings.Category));
    }
}
