using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

using DevExpress.CodeParser;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal sealed class FileSystemViewModel : MainViewModel {
        private readonly FileModelObjectConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public FileSystemViewModel(
            FileModelObjectConfig pluginConfig,
            RevitRepository revitRepository,
            IModelObjectService objectService,
            IOpenFolderDialogService openFolderDialogService,
            IProgressDialogFactory progressDialogFactory)
            : base(pluginConfig, objectService, openFolderDialogService, progressDialogFactory) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            IsExportRoomsVisible = true;
        }

        protected override void LoadConfigImpl() {
            IsExportRooms = _pluginConfig.IsExportRooms;
        }

        protected override void SaveConfigImpl() {
            _pluginConfig.IsExportRooms = IsExportRooms;
        }

        protected override void AcceptViewImpl() {
            Directory.CreateDirectory(TargetFolder);

            var navisFiles = Directory.GetFiles(TargetFolder, "*.nwc");
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
                }
            }
        }

        private void ExportDocument(string fileName) {
            _revitRepository.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
            _revitRepository.UIApplication.DialogBoxShowing += UIApplicationOnDialogBoxShowing;

            DocumentExtensions.UnloadAllLinks(fileName);
            Document document = _revitRepository.OpenDocumentFile(fileName);
            try {
                View3D navisView = new FilteredElementCollector(document)
                    .OfClass(typeof(View3D))
                    .OfType<View3D>()
                    .Where(item => !item.IsTemplate)
                    .FirstOrDefault(item =>
                        item.Name.Equals("Navisworks", StringComparison.OrdinalIgnoreCase));

                if(navisView == null) {
                    return;
                }

                var hasElements = new FilteredElementCollector(document, navisView.Id)
                        .WhereElementIsNotElementType()
#if REVIT_2021_OR_LESS
                        .Where(item =>
                            item.get_Geometry(new Options() {View = navisView})?.Any() == true)
#else
                        .WherePasses(new VisibleInViewFilter(document, navisView.Id))
#endif
                        .Any();

                if(!hasElements) {
                    return;
                }

                ProjectLocation[] projectLocations = document.ProjectLocations
                    .OfType<ProjectLocation>()
                    .ToArray();

                if(projectLocations.Length == 1) {
                    ExportDocument(fileName, navisView, document);
                } else if(projectLocations.Length > 1) {
                    foreach(ProjectLocation projectLocation in projectLocations) {
                        using(Transaction transaction = document.StartTransaction("Смена площадки")) {
                            document.ActiveProjectLocation = projectLocation;
                            transaction.Commit();
                        }

                        ExportDocument(fileName, navisView, document, projectLocation);
                    }
                }
            } finally {
                document.Close(false);
                _revitRepository.Application.FailuresProcessing -= ApplicationOnFailuresProcessing;
                _revitRepository.UIApplication.DialogBoxShowing -= UIApplicationOnDialogBoxShowing;
            }
        }

        private void ExportDocument(string fileName,
            View3D navisView,
            Document document,
            ProjectLocation location = null) {
            string exportFileName = IsExportRooms
                ? _revitRepository.GetRoomsFileName(fileName)
                : _revitRepository.GetFileName(fileName);

            NavisworksExportOptions exportOptions = IsExportRooms
                ? _revitRepository.GetRoomsExportOptions(navisView)
                : _revitRepository.GetExportOptions(navisView);

            exportFileName += location == null
                ? null
                : "_" + location.Name;

            document.Export(TargetFolder, exportFileName, exportOptions);
        }

        private void UIApplicationOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e) {
            e.OverrideResult(1);
        }

        private void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e) {
            FailuresAccessor accessor = e.GetFailuresAccessor();
            accessor.DeleteAllWarnings();
            accessor.ResolveFailures(accessor.GetFailureMessages());

            ElementId[] elementIds = accessor.GetFailureMessages()
                .SelectMany(item => item.GetFailingElementIds())
                .ToArray();

            if(elementIds.Length > 0) {
                accessor.DeleteElements(elementIds);
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            } else {
                e.SetProcessingResult(FailureProcessingResult.Continue);
            }
        }
    }
}
