using System;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

internal sealed class ManagerNavigationService(RevitRepository repository, ILocalizationService localization)
    : IManagerNavigationService {
    /// <inheritdoc />
    public string EnsureView(UIApplication app, ManagerConfigSnapshot config) {
        try {
            return repository.EnsureManagerView(app, config);
        } catch(Exception exception) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.ViewPreparationFailed"),
                exception);
        }
    }
}
