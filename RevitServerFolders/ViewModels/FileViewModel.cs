using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

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

            var modelFiles = ModelObjects
                .Where(item => !item.SkipObject)
                .Select(item => item.FullName);

            foreach(string fileName in modelFiles) {
                ExportDocument(fileName);
            }
        }

        private void ExportDocument(string fileName) {
            _revitRepository.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
            _revitRepository.UIApplication.DialogBoxShowing += UIApplicationOnDialogBoxShowing;

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
                    .Any(item =>
                        item.get_Geometry(new Options() {View = navisView})?.Any() == true);

                if(!hasElements) {
                    return;
                }

                string exportFileName = IsExportRooms
                    ? _revitRepository.GetRoomsFileName(fileName)
                    : _revitRepository.GetFileName(fileName);

                NavisworksExportOptions exportOptions = IsExportRooms
                    ? _revitRepository.GetRoomsExportOptions(navisView)
                    : _revitRepository.GetExportOptions(navisView);

                document.Export(TargetFolder, exportFileName, exportOptions);
            } finally {
                document.Close(false);
                _revitRepository.Application.FailuresProcessing -= ApplicationOnFailuresProcessing;
                _revitRepository.UIApplication.DialogBoxShowing -= UIApplicationOnDialogBoxShowing;
            }
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
