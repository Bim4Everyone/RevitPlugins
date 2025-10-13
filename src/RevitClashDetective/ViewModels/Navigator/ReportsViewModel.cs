using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.RevitClashReport;
using RevitClashDetective.Services.RevitViewSettings;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly SettingsConfig _settingsConfig;
    private bool _elementsIsolationEnabled = true;
    private bool _openFromClashDetector;
    private ReportViewModel _selectedFile;
    private ObservableCollection<ReportViewModel> _reports;

    public ReportsViewModel(RevitRepository revitRepository,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService,
        SettingsConfig settingsConfig,
        string selectedFile = null) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _settingsConfig = settingsConfig ?? throw new ArgumentNullException(nameof(settingsConfig));
        Reports = [];

        if(selectedFile == null) {
            InitializeFiles();
        } else {
            InitializeFiles(selectedFile);
        }

        OpenClashDetectorCommand = RelayCommand.Create(OpenClashDetector, () => OpenFromClashDetector);
        LoadCommand = RelayCommand.Create(Load);
        DeleteCommand = RelayCommand.Create(Delete, CanDelete);
        SelectClashCommand = RelayCommand.Create<IClashViewModel>(SelectClash, CanSelectClash);
        SaveAllReportsCommand = RelayCommand.Create(SaveAllReports, CanSaveAllReports);
    }


    public ICommand SelectClashCommand { get; }
    public ICommand OpenClashDetectorCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveAllReportsCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

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

    public bool ElementsIsolationEnabled {
        get => _elementsIsolationEnabled;
        set => RaiseAndSetIfChanged(ref _elementsIsolationEnabled, value);
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


    private void Load() {
        if(!OpenFileDialogService.ShowDialog(_revitRepository.GetFileDialogPath())) {
            throw new OperationCanceledException();
        }

        InitializeClashes(OpenFileDialogService.File.FullName);
        _revitRepository.CommonConfig.LastRunPath = OpenFileDialogService.File.DirectoryName;
        _revitRepository.CommonConfig.SaveProjectConfig();
    }

    private void InitializeClashes(string path) {
        string name = Path.GetFileNameWithoutExtension(path);
        var reports = ReportLoader.GetReports(_revitRepository, path)
            .Select(r => new ReportViewModel(
                _revitRepository,
                r.Name,
                _localizationService,
                OpenFileDialogService,
                SaveFileDialogService,
                MessageBoxService,
                _settingsConfig,
                r.Clashes?.ToArray() ?? []));

        Reports = new ObservableCollection<ReportViewModel>(new NameResolver<ReportViewModel>(Reports, reports).GetCollection());
        SelectedReport = Reports.FirstOrDefault();
    }

    private void Delete() {
        if(MessageBoxService.Show("Вы уверены, что хотите удалить файл?", "BIM",
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
        IView3DSetting settings;
        settings = ElementsIsolationEnabled
            ? new ClashIsolationViewSettings(_revitRepository, _localizationService, clash, _settingsConfig)
            : new ClashDefaultViewSettings(_revitRepository, _localizationService, clash, _settingsConfig);
        _revitRepository.SelectAndShowElement(clash.GetElements(), settings);
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
}
