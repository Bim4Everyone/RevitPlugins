using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class PrioritiesViewModel : BaseViewModel {
        private readonly MainViewModel _mainViewModel;
        private string _filePath;

        private PrioritiesConfig _prioritiesConfig;
        private List<PriorityViewModel> _prioritiesVM;


        public PrioritiesViewModel(MainViewModel mainViewModel) {
            _mainViewModel = mainViewModel;

            SetDefaultConfig(new object());

            SetDefaultConfigCommand = new RelayCommand(SetDefaultConfig);
            ImportConfigCommand = new RelayCommand(ImportConfig);
            ExportConfigCommand = new RelayCommand(ExportConfig);
        }

        public ICommand SetDefaultConfigCommand { get; }
        public ICommand ImportConfigCommand { get; }
        public ICommand ExportConfigCommand { get; }

        public List<PriorityViewModel> PrioritiesVM {
            get => _prioritiesVM;
            set => RaiseAndSetIfChanged(ref _prioritiesVM, value);
        }

        public string FilePath {
            get => _filePath;
            set => RaiseAndSetIfChanged(ref _filePath, value);
        }

        public PrioritiesConfig PrioritiesConfig => _prioritiesConfig;

        public void SetDefaultConfig(object obj) {
            _prioritiesConfig = PrioritiesConfig.GetDefaultConfig();

            _mainViewModel.CanLoadUtp = true;
            _mainViewModel.CanLoadUtpText = "";
            FilePath = "";

            PrioritiesVM = _prioritiesConfig
                .Priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x))
                .ToList();
        }

        public void ImportConfig(object obj) {
            OpenFileDialog dialog = new OpenFileDialog() {
               Title = "Выберите Json файл",
               Filter = "json файлы (*.json)|*.json"
            };

            if(dialog.ShowDialog() == DialogResult.OK) {
                SetConfigFromPath(dialog.FileName);
            }
        }

        public void ExportConfig(object obj) {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog() { 
                Filters = { new CommonFileDialogFilter("Json", "*.json") }
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                JsonExporter<RoomPriority> exporter = new JsonExporter<RoomPriority>();
                exporter.Export(dialog.FileName, _prioritiesConfig.Priorities);
                Autodesk.Revit.UI.TaskDialog.Show(
                    "Декларации", 
                    "Файл конфигурации приоритетов создан");
            }
        }

        public void SetConfigFromPath(string path) {
            FilePath = path;
            JsonImporter<RoomPriority> importer = new JsonImporter<RoomPriority>();
            List<RoomPriority> priorities = importer.Import(path);

            if(priorities.Any()) {
                PrioritiesVM = priorities
                    .OrderBy(x => x.OrdinalNumber)
                    .Select(x => new PriorityViewModel(x))
                    .ToList();

                _mainViewModel.LoadUtp = false;
                _mainViewModel.CanLoadUtp = false;
                _mainViewModel.CanLoadUtpText = "Выгрузка доступна только с приоритетами A101";

                _prioritiesConfig = new PrioritiesConfig(PrioritiesVM
                                            .Select(x => x.Priority)
                                            .ToList());
            } else {
                Autodesk.Revit.UI.TaskDialog.Show("Импорт приоритетов", importer.ErrorInfo);
            }
        }
    }
}
