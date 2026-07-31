using System;

using Ninject;
using Ninject.Syntax;

using Wpf.Ui.Abstractions;

namespace RevitLintelsManager.Models;

internal sealed class NavigationViewPageProvider(IResolutionRoot resolutionRoot) : INavigationViewPageProvider {
    public object GetPage(Type pageType) {
        return resolutionRoot.Get(pageType);
    }
}
