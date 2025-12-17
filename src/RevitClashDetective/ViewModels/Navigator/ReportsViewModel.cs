using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.RevitClashReport;
using RevitClashDetective.Services.RevitViewSettings;

using Wpf.Ui;

using IMessageBoxService = dosymep.SimpleServices.IMessageBoxService;
using IOpenFileDialogService = dosymep.SimpleServices.IOpenFileDialogService;
using ISaveFileDialogService = dosymep.SimpleServices.ISaveFileDialogService;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportsViewModel : BaseViewModel, ISupportServices {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IContentDialogService _contentDialogService;
    private readonly SettingsConfig _settingsConfig;
    private bool _openFromClashDetector;
    private ReportViewModel _selectedFile;
    private ObservableCollection<ReportViewModel> _reports;

    public ReportsViewModel(RevitRepository revitRepository,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService,
        IContentDialogService contentDialogService,
        SettingsConfig settingsConfig,
        string selectedFile = null) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _contentDialogService = contentDialogService ?? throw new ArgumentNullException(nameof(contentDialogService));
        _settingsConfig = settingsConfig ?? throw new ArgumentNullException(nameof(settingsConfig));
        Reports = [];

        if(selectedFile == null) {
            InitializeFiles();
        } else {
            InitializeFiles(selectedFile);
        }

        ServiceContainer = new ServiceContainer(this);
        OpenClashDetectorCommand = RelayCommand.Create(OpenClashDetector, CanOpenClashDetector);
        LoadCommand = RelayCommand.Create(Load);
        DeleteCommand = RelayCommand.Create(Delete, CanDelete);
        SelectClashCommand = RelayCommand.Create<IClashViewModel>(SelectClash, CanSelectClash);
        EditCommentsCommand = RelayCommand.Create<IClashViewModel>(EditComments, CanEditComments);
        SaveAllReportsCommand = RelayCommand.Create(SaveAllReports, CanSaveAllReports);
        AskForSaveCommand = RelayCommand.Create(AskForSave);
    }


    public ICommand SelectClashCommand { get; }
    public ICommand OpenClashDetectorCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveAllReportsCommand { get; }
    public ICommand AskForSaveCommand { get; }
    public ICommand EditCommentsCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IServiceContainer ServiceContainer { get; }


    public ObservableCollection<ReportViewModel> Reports {
        get => _reports;
        set => RaiseAndSetIfChanged(ref _reports, value);
    }

    public bool OpenFromClashDetector {
        get => _openFromClashDetector;
        set => RaiseAndSetIfChanged(ref _openFromClashDetector, value);
    }

    public ReportViewModel SelectedReport {
        get => _selectedFile;
        set => RaiseAndSetIfChanged(ref _selectedFile, value);
    }


    private void InitializeFiles(string selectedFile) {
        string profilePath = RevitRepository.LocalProfilePath;
        Reports = new ObservableCollection<ReportViewModel>(Directory.GetFiles(Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName()))
            .Select(path => new ReportViewModel(
                _revitRepository,
                Path.GetFileNameWithoutExtension(path),
                _localizationService,
                OpenFileDialogService,
                SaveFileDialogService,
                MessageBoxService,
                _contentDialogService,
                _settingsConfig))
            .Where(item => item.Name.Equals(selectedFile, StringComparison.CurrentCultureIgnoreCase)));
        SelectedReport = Reports.FirstOrDefault();
    }

    private void InitializeFiles() {
        string profilePath = RevitRepository.LocalProfilePath;
        string path = Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName());
        if(Directory.Exists(path)) {
            Reports = new ObservableCollection<ReportViewModel>(Directory.GetFiles(path)
                .Select(item => new ReportViewModel(
                    _revitRepository,
                    Path.GetFileNameWithoutExtension(item),
                    _localizationService,
                    OpenFileDialogService,
                    SaveFileDialogService,
                    MessageBoxService,
                    _contentDialogService,
                    _settingsConfig)));
            SelectedReport = Reports.FirstOrDefault();
        }
    }

    private void OpenClashDetector() {
        _revitRepository.DoAction(() => {
            var command = new DetectiveClashesCommand();
            command.ExecuteCommand(_revitRepository.UiApplication);
        });
    }

    private bool CanOpenClashDetector() {
        return OpenFromClashDetector;
    }

    private void AskForSave() {
        if(MessageBoxService.Show(_localizationService.GetLocalizedString("Navigator.SavePrompt"),
            _localizationService.GetLocalizedString("BIM"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes) {
            SaveAllReports();
        }
    }

    private void Load() {
        string path = _revitRepository.GetFileDialogPath();
        if(!OpenFileDialogService.ShowDialog(path)) {
            throw new OperationCanceledException();
        }

        InitializeClashes(OpenFileDialogService.File.FullName);
        _revitRepository.CommonConfig.LastRunPath = OpenFileDialogService.File.DirectoryName;
        _revitRepository.CommonConfig.SaveProjectConfig();
    }

    private void InitializeClashes(string path) {
        var reports = ReportLoader.GetReports(_revitRepository, path)
            .Select(r => new ReportViewModel(
                _revitRepository,
                r.Name,
                _localizationService,
                OpenFileDialogService,
                SaveFileDialogService,
                MessageBoxService,
                _contentDialogService,
                _settingsConfig,
                r.Clashes?.ToArray() ?? []))
            .ToArray();

        Reports = new ObservableCollection<ReportViewModel>(new ReportsNameResolver(_localizationService)
            .GetReports(Reports, reports));
        SelectedReport = null;
        SelectedReport = Reports.FirstOrDefault();
    }

    private void Delete() {
        if(MessageBoxService.Show(
            _localizationService.GetLocalizedString("Navigator.DeletePrompt"),
            _localizationService.GetLocalizedString("BIM"),
            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Yes) {
            DeleteConfig(SelectedReport.GetUpdatedConfig());
            Reports.Remove(SelectedReport);
            SelectedReport = Reports.FirstOrDefault();
        }
    }

    private void DeleteConfig(ClashesConfig config) {
        if(File.Exists(config.ProjectConfigPath) && config.ProjectConfigPath.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase)) {
            File.Delete(config.ProjectConfigPath);
        }
    }

    private bool CanDelete() {
        return SelectedReport != null;
    }

    private void SelectClash(IClashViewModel clash) {
        var settings = new ClashViewSettings(_revitRepository, _localizationService, clash, _settingsConfig);
        var elements = clash.GetElements();
        _revitRepository.SelectAndShowElement(elements, settings);
    }

    private bool CanSelectClash(IClashViewModel p) {
        return p != null && p.GetElements().Count > 0;
    }

    private void SaveAllReports() {
        foreach(var report in Reports) {
            report.SaveCommand.Execute(default);
        }
    }

    private bool CanSaveAllReports() {
        return Reports.Count > 0;
    }

    private void EditComments(IClashViewModel clashVm) {
        var clash = (ClashViewModel) clashVm;
        var dialogService = this.GetService<IDialogService>();
        dialogService.ShowDialog(
            dialogCommands: [
                new UICommand(
                    id: MessageBoxResult.OK,
                    caption: _localizationService.GetLocalizedString("Ok"),
                    command: null,
                    isDefault: true,
                    isCancel: false)
            ],
            title: _localizationService.GetLocalizedString("Navigator.ClashComments", clash.ClashName),
            viewModel: clash);
    }

    private bool CanEditComments(IClashViewModel clash) {
        return clash is ClashViewModel;
    }
}
