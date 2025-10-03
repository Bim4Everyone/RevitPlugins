using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal class NwcExportViewSettingsViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly IModelObjectService _rsOpenFileDialog;
    private string _errorText;
    private string _rvtFilePath;
    private string _viewTemplateName;
    private bool _canSave;
    private WorksetHideTemplate _selectedWorksetHideTemplate;


    public NwcExportViewSettingsViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialog,
        RsModelObjectService rsOpenFileDialog,
        NwcExportViewSettings settings) {

        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        OpenFileDialogService = openFileDialog ?? throw new System.ArgumentNullException(nameof(openFileDialog));
        _rsOpenFileDialog = rsOpenFileDialog ?? throw new System.ArgumentNullException(nameof(rsOpenFileDialog));
        if(settings is null) { throw new System.ArgumentNullException(nameof(settings)); }

        RvtFilePath = settings.RvtFilePath;
        ViewTemplateName = settings.ViewTemplateName;
        WorksetHideTemplates = new ObservableCollection<WorksetHideTemplate>(
            settings.WorksetHideTemplates
            .Select(t => new WorksetHideTemplate() { Template = t }) ?? []);
        SelectedWorksetHideTemplate = WorksetHideTemplates.FirstOrDefault();

        HelperCommand = RelayCommand.Create(() => { }, CanAcceptDialog);
        SelectRsFileCommand = RelayCommand.CreateAsync(SelectRsFileAsync, CanSelectFile);
        SelectLocalFileCommand = RelayCommand.Create(SelectLocalFile, CanSelectFile);
        AddWorksetHideTemplateCommand = RelayCommand.Create(AddWorksetHideTemplate);
        RemoveWorksetHideTemplateCommand
            = RelayCommand.Create<WorksetHideTemplate>(RemoveWorksetHideTemplate, CanRemoveWorksetHideTemplate);
    }

    /// <summary>
    /// Команда, чтобы делать валидацию aka CanAcceptView у главного окна
    /// </summary>
    public ICommand HelperCommand { get; }

    public IAsyncCommand SelectRsFileCommand { get; }

    public ICommand SelectLocalFileCommand { get; }

    public ICommand AddWorksetHideTemplateCommand { get; }

    public ICommand RemoveWorksetHideTemplateCommand { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public ObservableCollection<WorksetHideTemplate> WorksetHideTemplates { get; }

    public WorksetHideTemplate SelectedWorksetHideTemplate {
        get => _selectedWorksetHideTemplate;
        set => RaiseAndSetIfChanged(ref _selectedWorksetHideTemplate, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string RvtFilePath {
        get => _rvtFilePath;
        set => RaiseAndSetIfChanged(ref _rvtFilePath, value);
    }

    public string ViewTemplateName {
        get => _viewTemplateName;
        set => RaiseAndSetIfChanged(ref _viewTemplateName, value);
    }

    public bool CanSave {
        get => _canSave;
        set => RaiseAndSetIfChanged(ref _canSave, value);
    }


    public NwcExportViewSettings GetSettings() {
        return new NwcExportViewSettings() {
            RvtFilePath = RvtFilePath,
            ViewTemplateName = ViewTemplateName,
            WorksetHideTemplates = [.. WorksetHideTemplates.Select(t => t.Template)]
        };
    }

    private void AddWorksetHideTemplate() {
        WorksetHideTemplates.Add(new WorksetHideTemplate());
    }

    private void RemoveWorksetHideTemplate(WorksetHideTemplate template) {
        WorksetHideTemplates.Remove(template);
        SelectedWorksetHideTemplate = WorksetHideTemplates.FirstOrDefault();
    }

    private bool CanRemoveWorksetHideTemplate(WorksetHideTemplate template) {
        return template is not null;
    }

    private void SelectLocalFile() {
        if(OpenFileDialogService.ShowDialog()) {
            RvtFilePath = OpenFileDialogService.File.FullName;
        }
    }

    private async Task SelectRsFileAsync() {
        var modelObject = await _rsOpenFileDialog.SelectModelObjectDialog();
        RvtFilePath = modelObject.FullName;
        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanSelectFile() {
        return !SelectRsFileCommand.IsExecuting;
    }

    private bool CanAcceptDialog() {
        if(string.IsNullOrWhiteSpace(RvtFilePath)) {
            ErrorText = _localization.GetLocalizedString(
                "NwcExportViewSettingsDialog.Validation.RvtPathNotSet");
            CanSave = false;
            return false;
        }
        if(string.IsNullOrWhiteSpace(ViewTemplateName)) {
            ErrorText = _localization.GetLocalizedString(
                "NwcExportViewSettingsDialog.Validation.ViewNameNotSet");
            CanSave = false;
            return false;
        }
        if(WorksetHideTemplates.Any(t => string.IsNullOrWhiteSpace(t.Template))) {
            ErrorText = _localization.GetLocalizedString(
                "NwcExportViewSettingsDialog.Validation.WorksetTemplateEmpty");
            CanSave = false;
            return false;
        }
        var repeatedTemplate = WorksetHideTemplates
            .GroupBy(t => t.Template)
            .FirstOrDefault(g => g.Count() > 1);
        if(repeatedTemplate is not null) {
            ErrorText = _localization.GetLocalizedString(
                "NwcExportViewSettingsDialog.Validation.WorksetTemplatesDuplicated",
                repeatedTemplate.Key);
            CanSave = false;
            return false;
        }
        ErrorText = null;
        CanSave = true;
        return true;
    }
}
