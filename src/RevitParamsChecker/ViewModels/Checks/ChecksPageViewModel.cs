using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Checks;

internal class ChecksPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly FiltersRepository _filtersRepo;
    private readonly RulesRepository _rulesRepo;
    private readonly ChecksRepository _checksRepo;
    private readonly RevitRepository _revitRepo;
    private readonly ChecksConverter _checksConverter;
    private readonly NameEditorService _nameEditorService;
    private bool _allSelected;
    private string[] _availableFiles;
    private string[] _availableFilters;
    private string[] _availableRules;
    private CheckViewModel _selectedCheck;

    public ChecksPageViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        FiltersRepository filtersRepo,
        RulesRepository rulesRepo,
        ChecksRepository checksRepo,
        RevitRepository revitRepo,
        ChecksConverter checksConverter,
        NameEditorService nameEditorService) {
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _rulesRepo = rulesRepo ?? throw new ArgumentNullException(nameof(rulesRepo));
        _checksRepo = checksRepo ?? throw new ArgumentNullException(nameof(checksRepo));
        _revitRepo = revitRepo ?? throw new ArgumentNullException(nameof(revitRepo));
        _checksConverter = checksConverter ?? throw new ArgumentNullException(nameof(checksConverter));
        _nameEditorService = nameEditorService ?? throw new ArgumentNullException(nameof(nameEditorService));

        _availableFiles = [.._revitRepo.GetDocuments().Select(d => d.Name)];
        _availableFilters = [.._filtersRepo.GetFilters().Select(f => f.Name)];
        _availableRules = [.._rulesRepo.GetRules().Select(r => r.Name)];

        Checks = [.._checksRepo.GetChecks().Select(c => new CheckViewModel(c)).ToArray()];
        SelectedCheck = Checks.FirstOrDefault();
        AddCheckCommand = RelayCommand.Create(AddCheck);
        RenameCheckCommand = RelayCommand.Create<CheckViewModel>(RenameCheck, CanRenameCheck);
        RemoveChecksCommand = RelayCommand.Create<IList>(RemoveChecks, CanRemoveChecks);
        CopyCheckCommand = RelayCommand.Create<CheckViewModel>(CopyCheck, CanCopyCheck);
        LoadCommand = RelayCommand.Create(Load);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
        ExecuteChecksCommand = RelayCommand.Create(ExecuteChecks, CanExecuteChecks);
        SetCheckSelectedFilesCommand = RelayCommand.CreateAsync<CheckViewModel>(SetSelectedFilesAsync, CanSetItems);
        SetCheckSelectedFiltersCommand = RelayCommand.CreateAsync<CheckViewModel>(SetSelectedFiltersAsync, CanSetItems);
        SetCheckSelectedRulesCommand = RelayCommand.CreateAsync<CheckViewModel>(SetSelectedRulesAsync, CanSetItems);

        _filtersRepo.FiltersChanged += FiltersChangedHandler;
        _rulesRepo.RulesChanged += RulesChangedHandler;
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand AddCheckCommand { get; }
    public ICommand CopyCheckCommand { get; }
    public ICommand RenameCheckCommand { get; }
    public ICommand RemoveChecksCommand { get; }
    public ICommand ExecuteChecksCommand { get; }
    public IAsyncCommand SetCheckSelectedFilesCommand { get; }
    public IAsyncCommand SetCheckSelectedFiltersCommand { get; }
    public IAsyncCommand SetCheckSelectedRulesCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }

    public ObservableCollection<CheckViewModel> Checks { get; }

    public CheckViewModel SelectedCheck {
        get => _selectedCheck;
        set => RaiseAndSetIfChanged(ref _selectedCheck, value);
    }

    public bool AllSelected {
        get => _allSelected;
        set => RaiseAndSetIfChanged(ref _allSelected, value);
    }

    private void AddCheck() {
        try {
            var newCheck = new Check();
            newCheck.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("ChecksPage.NewCheckPrompt"),
                Checks.Select(f => f.Name).ToArray());
            var vm = new CheckViewModel(newCheck);
            Checks.Add(vm);
            SelectedCheck = vm;
        } catch(OperationCanceledException) {
        }
    }

    private void RenameCheck(CheckViewModel check) {
        try {
            check.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("ChecksPage.RenameCheckPrompt"),
                Checks.Select(f => f.Name).ToArray(),
                check.Name);
        } catch(OperationCanceledException) {
        }
    }

    private void CopyCheck(CheckViewModel check) {
        try {
            var copyCheck = check.GetCheck().Copy();
            copyCheck.Name = _nameEditorService.CreateNewName(
                _localization.GetLocalizedString("ChecksPage.NewCheckPrompt"),
                Checks.Select(f => f.Name).ToArray(),
                check.Name);
            var vm = new CheckViewModel(copyCheck);
            Checks.Add(vm);
            SelectedCheck = vm;
        } catch(OperationCanceledException) {
        }
    }

    private bool CanCopyCheck(CheckViewModel check) {
        return check is not null; // TODO валидация
    }

    private bool CanRenameCheck(CheckViewModel filter) {
        return filter is not null;
    }

    private void RemoveChecks(IList items) {
        var filters = items.OfType<CheckViewModel>().ToArray();
        foreach(var filter in filters) {
            Checks.Remove(filter);
        }

        SelectedCheck = Checks.FirstOrDefault();
    }

    private bool CanRemoveChecks(IList items) {
        return items != null
               && items.OfType<CheckViewModel>().Count() != 0;
    }

    private void ExecuteChecks() {
        // TODO
    }

    private bool CanExecuteChecks() {
        return true; // TODO
    }

    private void Load() {
        // TODO
    }

    private void Save() {
        _checksRepo.SetChecks(Checks.Select(c => c.GetCheck()).ToArray());
    }

    private void SaveAs() {
        // TODO
    }

    private bool CanSave() {
        return true; // TODO
    }

    private async Task SetSelectedFilesAsync(CheckViewModel check) {
        try {
            var selectedFiles = await _nameEditorService.SelectNamesAsync(
                _localization.GetLocalizedString("ChecksPage.SetSelectedFilesPrompt"),
                _availableFiles.ToArray(),
                check.SelectedFiles.ToArray());
            check.SelectedFiles = [..selectedFiles];
        } catch(OperationCanceledException) {
        }
    }

    private async Task SetSelectedFiltersAsync(CheckViewModel check) {
        try {
            var selectedFilters = await _nameEditorService.SelectNamesAsync(
                _localization.GetLocalizedString("ChecksPage.SetSelectedFiltersPrompt"),
                _availableFilters.ToArray(),
                check.SelectedFilters.ToArray());
            check.SelectedFilters = [..selectedFilters];
        } catch(OperationCanceledException) {
        }
    }

    private async Task SetSelectedRulesAsync(CheckViewModel check) {
        try {
            var selectedRules = await _nameEditorService.SelectNamesAsync(
                _localization.GetLocalizedString("ChecksPage.SetSelectedRulesPrompt"),
                _availableRules.ToArray(),
                check.SelectedRules.ToArray());
            check.SelectedRules = [..selectedRules];
        } catch(OperationCanceledException) {
        }
    }

    private bool CanSetItems(CheckViewModel check) {
        return check is not null;
    }

    private void RulesChangedHandler(object sender, RulesChangedEventArgs e) {
        // TODO
        _availableRules = [..e.NewRules.Select(r => r.Name)];
    }

    private void FiltersChangedHandler(object sender, FiltersChangedEventArgs e) {
        // TODO
        _availableFilters = [..e.NewFilters.Select(f => f.Name)];
    }
}
