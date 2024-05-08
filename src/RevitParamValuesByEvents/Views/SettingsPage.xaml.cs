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

            // На случай, когда вкладку плагина закрыли, сначала пытаемся ее получить, потом регаем
            // Т.к. нет возможности запросить наличие зареганой панели, то пытаемся ее получить
            // Если ее нет, регестрируем и показываем
            try {

                DockablePane dockablePane = uIApplication.GetDockablePane(_dockablePaneId);
                dockablePane.Show();

            } catch(Exception) {

                uIApplication.RegisterDockablePane(_dockablePaneId, "Корректор свойств", this as IDockablePaneProvider);

                OpenNCloseTempDoc(uIApplication);
            }
        }

        /// <summary>
        /// Метод автоматически вызывающийся при создании встраиваемой панели
        /// </summary>
        public void SetupDockablePane(DockablePaneProviderData data) {
            data.FrameworkElement = this as FrameworkElement;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);

            data.InitialState = new DockablePaneState {

                TabBehind = DockablePanes.BuiltInDockablePanes.PropertiesPalette,
                DockPosition = DockPosition.Tabbed
            };
        }


        /// <summary>
        /// Метод использующийся для перезагрузки UIApplication Revit, т.к. API не предоставляет иного.
        /// Перезагрузка UI приложения необходима для отображения встраиваемой панели, т.к. при ее создании применяется не стандартная методика
        /// Создает временный файл по пути хранения настроек плагина, открывает его, закрывает и удаляет.
        /// </summary>
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
