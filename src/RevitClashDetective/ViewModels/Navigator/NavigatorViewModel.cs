using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.UI;

using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.RevitClashReport;
using RevitClashDetective.Services.ReportsMerging;
using RevitClashDetective.Services.RevitViewSettings;

using Wpf.Ui;

using IMessageBoxService = dosymep.SimpleServices.IMessageBoxService;
using IOpenFileDialogService = dosymep.SimpleServices.IOpenFileDialogService;
using ISaveFileDialogService = dosymep.SimpleServices.ISaveFileDialogService;

namespace RevitClashDetective.ViewModels.Navigator;

internal class NavigatorViewModel : BaseViewModel, ISupportServices {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IContentDialogService _contentDialogService;
    private readonly SettingsConfig _settingsConfig;
    private bool _openFromClashDetector;
    private ReportViewModel _selectedFile;
    private ObservableCollection<ReportViewModel> _reports;
    private ReportsMergeViewModel _reportsMergeViewModel;
    private bool _showReportsMergeView;

    public NavigatorViewModel(
        RevitRepository revitRepository,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService,
        IContentDialogService contentDialogService,
        SettingsConfig settingsConfig,
        string reportName = null) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _contentDialogService = contentDialogService ?? throw new ArgumentNullException(nameof(contentDialogService));
        _settingsConfig = settingsConfig ?? throw new ArgumentNullException(nameof(settingsConfig));
        Reports = [];

        if(reportName == null) {
            InitializeReports();
        } else {
            InitializeReports(reportName);
        }

        ServiceContainer = new ServiceContainer(this);
        OpenClashDetectorCommand = RelayCommand.Create(OpenClashDetector, CanOpenClashDetector);
        LoadCommand = RelayCommand.Create(Load);
        DeleteCommand = RelayCommand.Create(Delete, CanDelete);
        SelectClashCommand = RelayCommand.Create<IClashViewModel>(SelectClash, CanSelectClash);
        EditCommentsCommand = RelayCommand.Create<IClashViewModel>(EditComments, CanEditComments);
        ShowCommentsCommand = RelayCommand.Create<IClashViewModel>(ShowComments, CanEditComments);
        AcceptReportsMergeCommand =
            RelayCommand.Create<ReportsMergeViewModel>(AcceptReportsMerge, CanAcceptReportsMerge);
        CancelReportsMergeCommand = RelayCommand.Create(CancelReportsMerge);
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
    public ICommand ShowCommentsCommand { get; }
    public ICommand AcceptReportsMergeCommand { get; }
    public ICommand CancelReportsMergeCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IServiceContainer ServiceContainer { get; }

    public ReportsMergeViewModel ReportsMergeViewModel {
        get => _reportsMergeViewModel;
        set => RaiseAndSetIfChanged(ref _reportsMergeViewModel, value);
    }

    public bool ShowReportsMergeView {
        get => _showReportsMergeView;
        set => RaiseAndSetIfChanged(ref _showReportsMergeView, value);
    }

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

    private void InitializeReports(string reportName = null) {
        string reportsDir = Path.Combine(
            RevitRepository.LocalProfilePath,
            ModuleEnvironment.RevitVersion,
            nameof(RevitClashDetective),
            _revitRepository.GetObjectName());

        if(!Directory.Exists(reportsDir)) {
            return;
        }

        ICollection<ReportViewModel> reports;
        if(reportName != null) {
            reports = GetReports(Path.Combine(reportsDir, reportName + ".json"));
        } else {
            reports = Directory.GetFiles(reportsDir, "*.json")
                .SelectMany(GetReports)
                .ToArray();
        }

        SetReports(reports);
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

    /// <summary>
    /// Загружает отчеты из заданного файла.
    /// </summary>
    /// <param name="reportPath">Путь к файлу отчета. Например, xml из нэвиса или json из ревита.</param>
    /// <returns>Коллекция отчетов из заданного файла.</returns>
    private IList<ReportViewModel> GetReports(string reportPath) {
        return ReportLoader.GetReports(_revitRepository, reportPath)
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
    }

    private void Load() {
        string path = _revitRepository.GetFileDialogPath();
        if(!OpenFileDialogService.ShowDialog(path)) {
            throw new OperationCanceledException();
        }

        var existingReportsSet = new ReportSet(Reports);
        var importingReports = GetReports(path);
        var importingReportsSet = new ReportSet(importingReports);
        var reportsIntersection = existingReportsSet.Intersect(importingReportsSet);
        var namesResolver = new ReportsIntersectionResolver();
        if(reportsIntersection.HasIntersection) {
            switch(GetLoadMode(string.Join(", ", reportsIntersection.LeftInnerSet.Reports.Select(r => r.Name)))) {
                case TaskDialogResult.CommandLink1: {
                    StartReportsMerge(reportsIntersection);
                    break;
                }
                case TaskDialogResult.CommandLink2: {
                    SetReports(namesResolver.Replace(reportsIntersection));
                    break;
                }
                case TaskDialogResult.CommandLink3: {
                    SetReports(namesResolver.CopyAndRename(reportsIntersection));
                    break;
                }
                default: {
                    // отмена
                    return;
                }
            }
        } else {
            SetReports(Reports.Union(importingReports));
        }

        _revitRepository.CommonConfig.LastRunPath = OpenFileDialogService.File.DirectoryName;
        _revitRepository.CommonConfig.SaveProjectConfig();
    }

    private void SetReports(IEnumerable<ReportViewModel> reports) {
        Reports = new ObservableCollection<ReportViewModel>(reports);
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

    private void ShowComments(IClashViewModel clashVm) {
        var clash = (ClashViewModel) clashVm;
        bool backup = clash.CanEditComments;
        clash.CanEditComments = false;
        EditComments(clash);
        clash.CanEditComments = backup;
    }

    private TaskDialogResult GetLoadMode(string names) {
        var dialog = new TaskDialog(_localizationService.GetLocalizedString("ReportsResolver.Header")) {
            MainContent = _localizationService.GetLocalizedString("ReportsResolver.Body", names)
        };
        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink1,
            _localizationService.GetLocalizedString("ReportsResolver.Merge"));
        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink2,
            _localizationService.GetLocalizedString("ReportsResolver.Replace"));
        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink3,
            _localizationService.GetLocalizedString("ReportsResolver.CopyAndRename"));
        dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
        return dialog.Show();
    }

    private void StartReportsMerge(ReportSetsIntersectionResult reportsIntersection) {
        ReportsMergeViewModel = new ReportsMergeViewModel(_localizationService, reportsIntersection);
        ShowReportsMergeView = true;
    }

    private void AcceptReportsMerge(ReportsMergeViewModel vm) {
        SetReports(vm.GetMergeResult());
        ShowReportsMergeView = false;
        ReportsMergeViewModel = null;
    }

    private bool CanAcceptReportsMerge(ReportsMergeViewModel vm) {
        if(vm is null) {
            return false;
        }

        return vm.AcceptMergeCommand.CanExecute(null);
    }

    private void CancelReportsMerge() {
        ShowReportsMergeView = false;
        ReportsMergeViewModel = null;
    }
}
