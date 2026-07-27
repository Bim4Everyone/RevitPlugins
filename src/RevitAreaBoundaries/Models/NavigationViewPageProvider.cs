using System;

using Ninject;
using Ninject.Syntax;

using Wpf.Ui.Abstractions;

namespace RevitAreaBoundaries.Models;

internal sealed class NavigationViewPageProvider(IResolutionRoot resolutionRoot) : INavigationViewPageProvider {
    public object GetPage(Type pageType) {
        return resolutionRoot.Get(pageType);
    }
}
