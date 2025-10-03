using System;
using System.Threading;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitServerFolders.Models;
using RevitServerFolders.ViewModels;
using RevitServerFolders.Views;

namespace RevitServerFolders.Services;
internal class NwcExportViewSettingsService : INwcExportViewSettingsService {
    private readonly IResolutionRoot _root;

    public NwcExportViewSettingsService(IResolutionRoot root) {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }


    public async Task<NwcExportViewSettingsDialogResult> ShowDialogAsync(
        NwcExportViewSettings currentSettings,
        CancellationToken ct = default) {
        if(currentSettings is null) {
            throw new ArgumentNullException(nameof(currentSettings));
        }

        var settingsParameter = new Ninject.Parameters.ConstructorArgument("settings", currentSettings);
        var vm = _root.Get<NwcExportViewSettingsViewModel>(settingsParameter);
        var vmParameter = new Ninject.Parameters.ConstructorArgument("vm", vm);
        var dialog = _root.Get<NwcExportViewSettingsDialog>(vmParameter);

        var dialogResult = await dialog.ShowAsync(ct);
        if(dialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary) {
            return new NwcExportViewSettingsDialogResult() {
                IsSuccess = true,
                Settings = dialog.NwcExportViewSettings.GetSettings()
            };
        } else {
            return new NwcExportViewSettingsDialogResult() {
                IsSuccess = false
            };
        }
    }
}
