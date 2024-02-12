using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal sealed class RsViewModel : MainViewModel {
        private readonly RsModelObjectConfig _pluginConfig;

        public RsViewModel(RsModelObjectConfig pluginConfig,
            IModelObjectService objectService,
            IOpenFolderDialogService openFolderDialogService,
            IProgressDialogFactory progressDialogFactory)
            : base(pluginConfig, objectService, openFolderDialogService, progressDialogFactory) {
            _pluginConfig = pluginConfig;
            IsExportRoomsVisible = false;
        }

        protected override void AcceptViewImpl() {
            var navisFiles = Directory.GetFiles(TargetFolder, "*.rvt");
            foreach(string navisFile in navisFiles) {
                File.SetAttributes(navisFile, FileAttributes.Normal);
                File.Delete(navisFile);
            }

            string[] modelFiles = ModelObjects
                .Where(item => !item.SkipObject)
                .Select(item => item.FullName)
                .ToArray();

            using(IProgressDialogService dialog = ProgressDialogFactory.CreateDialog()) {
                dialog.Show();
                dialog.StepValue = 1;
                dialog.MaxValue = modelFiles.Length;

                IProgress<int> progress = dialog.CreateProgress();
                CancellationToken cancellationToken = dialog.CreateCancellationToken();
                int count = 0;
                foreach(string fileName in modelFiles) {
                    progress.Report(++count);
                    cancellationToken.ThrowIfCancellationRequested();

                    ExportDocument(fileName);
                    dosymep.Revit.DocumentExtensions.UnloadAllLinks(Directory.GetFiles(TargetFolder, "*.rvt"));
                }
            }
        }

        private void ExportDocument(string modelName) {
            Uri uri = new Uri(modelName);
            
            string arguments = 
                $@"createLocalRvt ""{uri.LocalPath.Trim('\\').Trim('/')}"" -s ""{uri.Host}"" -d ""{TargetFolder}/"" -o";
            
            string fileName =
                $@"C:\Program Files\Autodesk\Revit {ModuleEnvironment.RevitVersion}\RevitServerToolCommand\RevitServerTool.exe";

            Process process = Process.Start(new ProcessStartInfo() {
                Arguments = arguments,
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            });

            process?.WaitForExit();
        }
    }
}
