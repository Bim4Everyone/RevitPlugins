using System;
using System.Linq;
using System.Threading;

using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal sealed class RsViewModel : MainViewModel {
        private readonly RsModelObjectConfig _pluginConfig;
        private readonly IModelsExportService _exportService;

        public RsViewModel(RsModelObjectConfig pluginConfig,
            IModelObjectService objectService,
            IModelsExportService exportService,
            IOpenFolderDialogService openFolderDialogService,
            IProgressDialogFactory progressDialogFactory)
            : base(pluginConfig, objectService, openFolderDialogService, progressDialogFactory) {
            _pluginConfig = pluginConfig;
            _exportService = exportService;
            IsExportRoomsVisible = false;
        }

        protected override void AcceptViewImpl() {
            string[] modelFiles = ModelObjects
                .Where(item => !item.SkipObject)
                .Select(item => item.FullName)
                .ToArray();

            using(IProgressDialogService dialog = ProgressDialogFactory.CreateDialog()) {
                dialog.StepValue = 1;
                dialog.MaxValue = modelFiles.Length;
                IProgress<int> progress = dialog.CreateProgress();
                CancellationToken ct = dialog.CreateCancellationToken();
                dialog.Show();

                _exportService.ExportModelObjects(TargetFolder, modelFiles, progress, ct);
            }
        }
    }
}
