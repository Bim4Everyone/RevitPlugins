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

    /// <inheritdoc />
    public ManagerNavigationResult Navigate(UIApplication app, ManagerRequest request, ManagerSessionState state) {
        var elements = repository.GetNavigationElements(app, request);
        if(elements is null) {
            return ManagerNavigationResult.SourceUnavailable;
        }
        if(elements.Count == 0) {
            return ManagerNavigationResult.TargetsUnavailable;
        }

        using var sectionBox = repository.GetNavigationSectionBox(elements);
        if(sectionBox is null) {
            return ManagerNavigationResult.BoundsUnavailable;
        }

        string viewUniqueId = EnsureView(app, state.Config);
        return viewUniqueId is not null
            && repository.ShowNavigationTargets(app, viewUniqueId, elements, sectionBox, request, state)
                ? ManagerNavigationResult.Completed : ManagerNavigationResult.SourceUnavailable;
    }
}
