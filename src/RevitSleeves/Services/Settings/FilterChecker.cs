using Ninject;
using Ninject.Syntax;

using RevitClashDetective.Models.FilterModel;

using RevitSleeves.ViewModels.Settings;
using RevitSleeves.Views.Settings;

namespace RevitSleeves.Services.Settings;
internal class FilterChecker : IFilterChecker {
    private readonly IResolutionRoot _resolutionRoot;

    public FilterChecker(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
    }


    public void ShowFilter(Filter filter) {
        var filterParameter = new Ninject.Parameters.ConstructorArgument(nameof(FilterCheckerViewModel.Filter), filter);
        var vm = _resolutionRoot.Get<FilterCheckerViewModel>(filterParameter);
        var window = _resolutionRoot.Get<FilterCheckerWindow>();
        window.DataContext = vm;
        window.Show();
    }
}
