using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class PrioritiesViewModel : BaseViewModel {
    private readonly MainViewModel _mainViewModel;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private string _filePath;
    private List<PriorityViewModel> _prioritiesVM;

    public PrioritiesViewModel(MainViewModel mainViewModel,
                               ILocalizationService localizationService,
                               IMessageBoxService messageBoxService) {
        _mainViewModel = mainViewModel;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;

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
            .Select(x => new PriorityViewModel(x, _localizationService))
            .ToList();
    }

    public void ImportConfig(object obj) {
        var dialog = new OpenFileDialog() {
            Title = _localizationService.GetLocalizedString("OpenFileDialog.SelectJson"),
            Filter = "json (*.json)|*.json"
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

            _messageBoxService.Show(
                _localizationService.GetLocalizedString("MessageBox.PrioritiesConfigCreated"),
                _localizationService.GetLocalizedString("MainWindow.Title"));
        }
    }

    public void SetConfigFromPath(string path) {
        FilePath = path;
        var importer = new JsonImporter<RoomPriority>();
        var priorities = importer.Import(path);

        if(priorities.Any()) {
            PrioritiesVM = priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x, _localizationService))
                .ToList();

            _mainViewModel.DeclarationViewModel.LoadUtp = false;
            _mainViewModel.DeclarationViewModel.CanLoadUtp = false;
            _mainViewModel.DeclarationViewModel.CanLoadUtpText = 
                _localizationService.GetLocalizedString("MainWindow.ErrorNoCorpPriorities");

            PrioritiesConfig = new PrioritiesConfig(PrioritiesVM
                                        .Select(x => x.Priority)
                                        .ToList());
        } else {
            _messageBoxService.Show(importer.ErrorInfo, _localizationService.GetLocalizedString("MainWindow.Title"));
        }
    }
}
