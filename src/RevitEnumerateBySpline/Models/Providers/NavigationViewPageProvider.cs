using System;

using Ninject;
using Ninject.Syntax;

using Wpf.Ui.Abstractions;

namespace RevitEnumerateBySpline.Models.Providers;

internal sealed class NavigationViewPageProvider(IResolutionRoot resolutionRoot) : INavigationViewPageProvider {
    public object GetPage(Type pageType) {
        return resolutionRoot.Get(pageType);
    }
}
