using System;
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

        public PrioritiesConfig PrioritiesConfig => _prioritiesConfig;

        public void SetDefaultConfig(object obj) {
            _prioritiesConfig = new DefaultPrioritiesConfig();

            _mainViewModel.CanLoadUtp = true;
            _mainViewModel.CanLoadUtpText = "";

            PrioritiesVM = _prioritiesConfig
                .Priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x))
                .ToList();
        }

        public void ImportConfig(object obj) {
            string path;

            OpenFileDialog dialog = new OpenFileDialog() {
               Title = "Выберите Json файл",
               Filter = "json файлы (*.json)|*.json"
            };

            if(dialog.ShowDialog() == DialogResult.OK) {
                path = dialog.FileName;
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

        public void ExportConfig(object obj) {
            string path;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog() {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                path = dialog.FileName + "\\ExportedConfig";
                JsonExporter<RoomPriority> exporter = new JsonExporter<RoomPriority>();
                exporter.Export(path, _prioritiesConfig.Priorities);
                Autodesk.Revit.UI.TaskDialog.Show(
                    "Декларации", 
                    "Файл конфигурации приоритетов создан");
            }
        }
    }
}
