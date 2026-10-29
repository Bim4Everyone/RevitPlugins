using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Syntax;

using RevitVORChecker.Models;
using RevitVORChecker.Models.Project;
using RevitVORChecker.Views;

namespace RevitVORChecker.ViewModels;

/// <summary>
/// Главное окно: параметры запуска проверки папки с моделями.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly IResolutionRoot _resolutionRoot;
    private readonly ILocalizationService _localizationService;
    private string _modelsFolder;
    private string _classifierPath;
    private string _clashTolerance;
    private bool _takeScreenshots;
    private bool _checkWorksets;
    private string _projectSummary;
    private string _errorText;
    private ProjectCard _projectCard;

    public MainViewModel(
        PluginConfig pluginConfig,
        IResolutionRoot resolutionRoot,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        IOpenFolderDialogService openFolderDialogService,
        IOpenFileDialogService openFileDialogService) {

        _pluginConfig = pluginConfig;
        _resolutionRoot = resolutionRoot;
        _localizationService = localizationService;
        MessageBoxService = messageBoxService;
        OpenFolderDialogService = openFolderDialogService;
        OpenFileDialogService = openFileDialogService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        SelectFolderCommand = RelayCommand.Create(SelectFolder);
        SelectClassifierCommand = RelayCommand.Create(SelectClassifier);
        EditProjectCommand = RelayCommand.Create(EditProject, CanEditProject);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    /// <summary>
    /// Окно просит закрыть себя: параметры приняты, проверка запускается после закрытия окна.
    /// </summary>
    public event EventHandler CloseRequested;

    /// <summary>
    /// Параметры запуска после нажатия «Запустить проверку».
    /// </summary>
    public CheckSettings Settings { get; private set; }

    public IMessageBoxService MessageBoxService { get; }

    public IOpenFolderDialogService OpenFolderDialogService { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public ICommand LoadViewCommand { get; }

    public ICommand SelectFolderCommand { get; }

    public ICommand SelectClassifierCommand { get; }

    public ICommand EditProjectCommand { get; }

    /// <summary>
    /// Запуск проверки.
    /// </summary>
    public ICommand AcceptViewCommand { get; }

    public string ModelsFolder {
        get => _modelsFolder;
        set {
            RaiseAndSetIfChanged(ref _modelsFolder, value);
            LoadProjectCard();
        }
    }

    /// <summary>
    /// Классификатор видов работ; пусто — найти в папке с моделями.
    /// </summary>
    public string ClassifierPath {
        get => _classifierPath;
        set => RaiseAndSetIfChanged(ref _classifierPath, value);
    }

    public string ClashTolerance {
        get => _clashTolerance;
        set => RaiseAndSetIfChanged(ref _clashTolerance, value);
    }

    public bool TakeScreenshots {
        get => _takeScreenshots;
        set => RaiseAndSetIfChanged(ref _takeScreenshots, value);
    }

    public bool CheckWorksets {
        get => _checkWorksets;
        set => RaiseAndSetIfChanged(ref _checkWorksets, value);
    }

    /// <summary>
    /// Краткое описание данных проекта.
    /// </summary>
    public string ProjectSummary {
        get => _projectSummary;
        set => RaiseAndSetIfChanged(ref _projectSummary, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        ClassifierPath = _pluginConfig.ClassifierPath;
        ClashTolerance = string.IsNullOrWhiteSpace(_pluginConfig.ClashToleranceMm) ? "50" : _pluginConfig.ClashToleranceMm;
        TakeScreenshots = _pluginConfig.TakeScreenshots;
        CheckWorksets = _pluginConfig.CheckWorksets;
        ModelsFolder = _pluginConfig.ModelsFolder;
    }

    private void SaveConfig() {
        _pluginConfig.ModelsFolder = ModelsFolder ?? "";
        _pluginConfig.ClassifierPath = ClassifierPath ?? "";
        _pluginConfig.ClashToleranceMm = ClashTolerance ?? "50";
        _pluginConfig.TakeScreenshots = TakeScreenshots;
        _pluginConfig.CheckWorksets = CheckWorksets;
        _pluginConfig.SaveProjectConfig();
    }

    private void SelectFolder() {
        if(OpenFolderDialogService.ShowDialog()) {
            ModelsFolder = OpenFolderDialogService.Folder.FullName;
        }
    }

    private void SelectClassifier() {
        if(OpenFileDialogService.ShowDialog()) {
            ClassifierPath = OpenFileDialogService.File.FullName;
        }
    }

    private void LoadProjectCard() {
        _projectCard = Directory.Exists(ModelsFolder) ? ProjectCardStorage.Load(ModelsFolder) : null;
        ProjectSummary = _projectCard != null
            ? _projectCard.GetSummary()
            : _localizationService.GetLocalizedString("MainWindow.ProjectNotSet");
    }

    private bool CanEditProject() {
        return Directory.Exists(ModelsFolder);
    }

    private void EditProject() {
        var window = _resolutionRoot.Get<ProjectCardWindow>();
        var projectCardViewModel = (ProjectCardViewModel) window.DataContext;
        if(_projectCard != null) {
            projectCardViewModel.SetData(_projectCard);
        }

        if(window.ShowDialog() != true) {
            return;
        }

        try {
            ProjectCardStorage.Save(ModelsFolder, projectCardViewModel.Result);
        } catch(Exception ex) {
            ShowMessage(
                _localizationService.GetLocalizedString("MainWindow.ProjectSaveError") + " " + ex.Message,
                MessageBoxImage.Error);
        }

        _projectCard = projectCardViewModel.Result;
        ProjectSummary = _projectCard.GetSummary();
    }

    private bool CanAcceptView() {
        if(!Directory.Exists(ModelsFolder)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoFolder");
            return false;
        }

        if(!string.IsNullOrWhiteSpace(ClassifierPath) && !File.Exists(ClassifierPath.Trim())) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoClassifier");
            return false;
        }

        if(!TryParseTolerance(out _)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.BadTolerance");
            return false;
        }

        if(_projectCard == null) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoProject");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private bool TryParseTolerance(out double tolerance) {
        string text = (ClashTolerance ?? "").Trim().Replace(",", ".");
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out tolerance) && tolerance > 0;
    }

    private void AcceptView() {
        SaveConfig();
        TryParseTolerance(out double tolerance);
        Settings = new CheckSettings {
            ModelsFolder = ModelsFolder,
            ClassifierPath = ClassifierPath,
            ClashToleranceMm = tolerance,
            TakeScreenshots = TakeScreenshots,
            CheckWorksets = CheckWorksets,
            Project = _projectCard
        };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ShowMessage(string message, MessageBoxImage image) {
        MessageBoxService.Show(
            message,
            _localizationService.GetLocalizedString("MainWindow.Title"),
            MessageBoxButton.OK,
            image,
            MessageBoxResult.OK);
    }
}
