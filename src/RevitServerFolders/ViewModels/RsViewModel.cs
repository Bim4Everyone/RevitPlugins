using System.Diagnostics;
using System.IO;
using System.Linq;

using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal sealed class RsViewModel : MainViewModel {
    private readonly RsModelObjectConfig _pluginConfig;
    private readonly IModelsExportService _exportService;

    public RsViewModel(RsModelObjectConfig pluginConfig,
        IModelObjectService objectService,
        IModelsExportService exportService,
        IOpenFolderDialogService openFolderDialogService,
        IProgressDialogFactory progressDialogFactory,
        ILocalizationService localization)
        : base(pluginConfig, objectService, openFolderDialogService, progressDialogFactory, localization) {

        _pluginConfig = pluginConfig;
        _exportService = exportService;
        IsExportRoomsVisible = false;
        Title = _localization.GetLocalizedString("RvtFromRsWindow.Title");
    }

    protected override void AcceptViewImpl() {
        string[] modelFiles = ModelObjects
            .Where(item => !item.SkipObject)
            .Select(item => item.FullName)
            .ToArray();

        using var dialog = ProgressDialogFactory.CreateDialog();
        dialog.StepValue = 1;
        dialog.MaxValue = modelFiles.Length;
        var progress = dialog.CreateProgress();
        var ct = dialog.CreateCancellationToken();
        dialog.Show();

        _exportService.ExportModelObjects(TargetFolder, modelFiles, ClearTargetFolder, progress, ct);

        if(OpenTargetWhenFinish) {
            Process.Start(Path.GetFullPath(TargetFolder));
        }
    }
}
