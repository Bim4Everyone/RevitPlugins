using System;

using Ninject;
using Ninject.Syntax;

using Wpf.Ui.Abstractions;

namespace RevitParamsChecker.Services;

internal class NavigationViewPageProvider : INavigationViewPageProvider {
    private readonly IResolutionRoot _root;

    public NavigationViewPageProvider(IResolutionRoot root) {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public object GetPage(Type pageType) {
        return _root.Get(pageType);
    }
}
