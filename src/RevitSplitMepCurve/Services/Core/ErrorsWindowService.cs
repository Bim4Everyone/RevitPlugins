using System;

using Ninject;
using Ninject.Syntax;

using RevitSplitMepCurve.Views;

namespace RevitSplitMepCurve.Services.Core;

internal class ErrorsWindowService {
    private readonly IResolutionRoot _root;

    public ErrorsWindowService(IResolutionRoot root) {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public void ShowErrorsWindow() {
        _root.Get<ErrorsWindow>().Show();
    }
}
