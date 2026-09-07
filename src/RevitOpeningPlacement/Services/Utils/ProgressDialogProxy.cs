using System;

using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Services.Utils;

internal class ProgressDialogProxy {
    private readonly IProgressDialogFactory _factory;

    public ProgressDialogProxy(IProgressDialogFactory factory) {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IProgressDialogService Create() {
        return _factory.CreateDialog();
    }
}
