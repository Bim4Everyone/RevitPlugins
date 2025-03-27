using System;

using Ninject;
using Ninject.Syntax;

using Wpf.Ui.Abstractions;

namespace RevitPlatformSettings.Factories;

internal sealed class NavigationViewPageProvider : INavigationViewPageProvider {
    private readonly IResolutionRoot _resolutionRoot;

    public NavigationViewPageProvider(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public object GetPage(Type pageType) {
        return _resolutionRoot.Get(pageType);
    }
}
