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
        private PrioritiesConfig _prioritiesConfig;
        private List<PriorityViewModel> _prioritiesVM;

        public PrioritiesViewModel() {
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
                List<RoomPriority> priorities = JsonImporter<RoomPriority>.Import(path);

                PrioritiesVM = priorities
                    .OrderBy(x => x.OrdinalNumber)
                    .Select(x => new PriorityViewModel(x))
                    .ToList();

                _prioritiesConfig = new PrioritiesConfig(PrioritiesVM
                                            .Select(x => x.Priority)
                                            .ToList());
            }
        }

        public void ExportConfig(object obj) {
            string path;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog() {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                path = dialog.FileName + "\\ExportedConfig";
                JsonExporter<RoomPriority>.Export(path, _prioritiesConfig.Priorities);
                TaskDialog.Show("Декларации", "Файл конфигурации приоритетов создан");
            }

        }
    }
}
