using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

namespace RevitParamValuesByEvents.Views {
    public partial class SettingsPage : Page, IDockablePaneProvider {

        private readonly string _pathToCallFile;
        private readonly DockablePaneId _dockablePaneId = new DockablePaneId(new Guid("{8019815A-EFC5-4C82-9462-338670C274C1}"));

        public SettingsPage(UIApplication uIApplication) {
            InitializeComponent();

            _pathToCallFile = uIApplication.ActiveUIDocument.Document.PathName;

            if(string.IsNullOrEmpty(_pathToCallFile)) {
                TaskDialog.Show("Ошибка!", "Необходимо сохранить проект перед запуском этого плагина!");
                return;
            }

            uIApplication.RegisterDockablePane(_dockablePaneId, "Корректор свойств", this as IDockablePaneProvider);

            OpenNCloseTempDoc(uIApplication);
        }


        //public void Dispose() {
        //    this.Dispose();
        //}

        public void SetupDockablePane(DockablePaneProviderData data) {
            data.FrameworkElement = this as FrameworkElement;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);

            data.InitialState = new DockablePaneState {

                TabBehind = DockablePanes.BuiltInDockablePanes.PropertiesPalette,
                DockPosition = DockPosition.Tabbed
            };
        }


        private void OpenNCloseTempDoc(UIApplication uIApplication) {

            // Формирование пути к временному файлу
            string revitVersion = ModuleEnvironment.RevitVersion;

            string pathToFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\dosymep\\{revitVersion}\\RevitParamValuesByEvents";
            string pathForTempFile = pathToFolder + $"\\TempFile.rvt";

            // Формирование временного файла
            if(!Directory.Exists(pathToFolder)) {
                Directory.CreateDirectory(pathToFolder);
            }

            Document tempDocument = uIApplication.Application.NewProjectDocument(UnitSystem.Metric);
            SaveAsOptions saveAsOptions = new SaveAsOptions();
            saveAsOptions.OverwriteExistingFile = true;
            saveAsOptions.MaximumBackups = 1;

            tempDocument.SaveAs(pathForTempFile, saveAsOptions);

            // Открытие с интерфейсом временного файла
            uIApplication.OpenAndActivateDocument(pathForTempFile);

            // Переход обратно к целевому файлу
            uIApplication.OpenAndActivateDocument(_pathToCallFile);

            // Закрытие и удаление временного файла
            tempDocument.Close(false);
            File.Delete(pathForTempFile);
        }
    }
}
