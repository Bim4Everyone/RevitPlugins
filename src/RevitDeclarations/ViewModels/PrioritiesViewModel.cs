using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class PrioritiesViewModel : BaseViewModel {
    private readonly MainViewModel _mainViewModel;
    private string _filePath;
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

    public PrioritiesConfig PrioritiesConfig { get; private set; }

    public void SetDefaultConfig(object obj) {
        PrioritiesConfig = PrioritiesConfig.GetDefaultConfig();

        _mainViewModel.DeclarationViewModel.CanLoadUtp = true;
        _mainViewModel.DeclarationViewModel.CanLoadUtpText = "";
        FilePath = "";

        PrioritiesVM = PrioritiesConfig
            .Priorities
            .OrderBy(x => x.OrdinalNumber)
            .Select(x => new PriorityViewModel(x))
            .ToList();
    }

    public void ImportConfig(object obj) {
        var dialog = new OpenFileDialog() {
            Title = "Выберите Json файл",
            Filter = "json файлы (*.json)|*.json"
        };

        if(dialog.ShowDialog() == DialogResult.OK) {
            SetConfigFromPath(dialog.FileName);
        }
    }

    public void ExportConfig(object obj) {
        var dialog = new CommonSaveFileDialog() {
            Filters = { new CommonFileDialogFilter("Json", "*.json") }
        };

        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            var exporter = new JsonExporter<RoomPriority>();
            exporter.Export(dialog.FileName, PrioritiesConfig.Priorities);
            Autodesk.Revit.UI.TaskDialog.Show(
                "Декларации",
                "Файл конфигурации приоритетов создан");
        }
    }

    public void SetConfigFromPath(string path) {
        FilePath = path;
        var importer = new JsonImporter<RoomPriority>();
        var priorities = importer.Import(path);

        if(priorities.Any()) {
            PrioritiesVM = priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x))
                .ToList();

            _mainViewModel.DeclarationViewModel.LoadUtp = false;
            _mainViewModel.DeclarationViewModel.CanLoadUtp = false;
            _mainViewModel.DeclarationViewModel.CanLoadUtpText = "Выгрузка доступна только с приоритетами A101";

            PrioritiesConfig = new PrioritiesConfig(PrioritiesVM
                                        .Select(x => x.Priority)
                                        .ToList());
        } else {
            Autodesk.Revit.UI.TaskDialog.Show("Импорт приоритетов", importer.ErrorInfo);
        }
    }
}
