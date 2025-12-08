using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
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
    private readonly NamesService _namesService;
    private readonly ChecksEngine _checksEngine;
    private string _dirPath;
    private bool _allSelected;
    private string[] _availableFiles;
    private string[] _availableFilters;
    private string[] _availableRules;
    private CheckViewModel _selectedCheck;

    public ChecksPageViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        IProgressDialogFactory progressDialogFactory,
        FiltersRepository filtersRepo,
        RulesRepository rulesRepo,
        ChecksRepository checksRepo,
        RevitRepository revitRepo,
        ChecksConverter checksConverter,
        NamesService namesService,
        ChecksEngine checksEngine) {
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        ProgressDialogFactory = progressDialogFactory ?? throw new ArgumentNullException(nameof(progressDialogFactory));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _rulesRepo = rulesRepo ?? throw new ArgumentNullException(nameof(rulesRepo));
        _checksRepo = checksRepo ?? throw new ArgumentNullException(nameof(checksRepo));
        _revitRepo = revitRepo ?? throw new ArgumentNullException(nameof(revitRepo));
        _checksConverter = checksConverter ?? throw new ArgumentNullException(nameof(checksConverter));
        _namesService = namesService ?? throw new ArgumentNullException(nameof(namesService));
        _checksEngine = checksEngine ?? throw new ArgumentNullException(nameof(checksEngine));
        _dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

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
        ExportCommand = RelayCommand.Create(Export, CanSave);
        ExecuteChecksCommand = RelayCommand.Create(ExecuteChecks, CanExecuteChecks);
        SetCheckSelectedFilesCommand = RelayCommand.CreateAsync<CheckViewModel>(SetSelectedFilesAsync, CanSetItems);
        SetCheckSelectedFiltersCommand = RelayCommand.CreateAsync<CheckViewModel>(SetSelectedFiltersAsync, CanSetItems);
        SetCheckSelectedRulesCommand = RelayCommand.CreateAsync<CheckViewModel>(SetSelectedRulesAsync, CanSetItems);

        _filtersRepo.FiltersChanged += FiltersChangedHandler;
        _rulesRepo.RulesChanged += RulesChangedHandler;
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExportCommand { get; }
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
    public IMessageBoxService MessageBoxService { get; }
    public IProgressDialogFactory ProgressDialogFactory { get; }

    public ObservableCollection<CheckViewModel> Checks { get; }

    public CheckViewModel SelectedCheck {
        get => _selectedCheck;
        set => RaiseAndSetIfChanged(ref _selectedCheck, value);
    }

    public bool AllSelected {
        get => _allSelected;
        set {
            if(_allSelected != value) {
                RaiseAndSetIfChanged(ref _allSelected, value);
                foreach(var check in Checks) {
                    check.IsSelected = value;
                }
            }
        }
    }

    private void AddCheck() {
        try {
            var newCheck = new Check();
            newCheck.Name = _namesService.CreateNewName(
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
            check.Name = _namesService.CreateNewName(
                _localization.GetLocalizedString("ChecksPage.RenameCheckPrompt"),
                Checks.Select(f => f.Name).ToArray(),
                check.Name);
        } catch(OperationCanceledException) {
        }
    }

    private void CopyCheck(CheckViewModel check) {
        try {
            var copyCheck = check.GetCheck().Copy();
            copyCheck.Name = _namesService.CreateNewName(
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
        using var progressDialogService = ProgressDialogFactory.CreateDialog();
        progressDialogService.Indeterminate = true;
        var ct = progressDialogService.CreateCancellationToken();
        progressDialogService.Show();

        var checks = Checks.Where(f => f.IsSelected).Select(c => c.GetCheck()).ToArray();
        foreach(var check in checks) {
            _checksEngine.Run(check, ct);
        }

        progressDialogService?.Close();
    }

    private bool CanExecuteChecks() {
        return Checks.Count(c => c.IsSelected) > 0; // TODO
    }

    private void Load() {
        if(OpenFileDialogService.ShowDialog(_dirPath)) {
            string filePath = OpenFileDialogService.File.FullName;
            string str = File.ReadAllText(filePath);
            Check[] checks;
            try {
                checks = _checksConverter.ConvertFromString(str);
            } catch(InvalidOperationException) {
                MessageBoxService.Show(
                    _localization.GetLocalizedString("ChecksPage.Error.CannotLoadChecks"));
                return;
            }

            var vms = _namesService.GetResolvedCollection(
                    Checks.ToArray(),
                    checks.Select(c => new CheckViewModel(c)).ToArray())
                .OfType<CheckViewModel>();
            Checks.Clear();
            foreach(var vm in vms) {
                Checks.Add(vm);
            }

            SelectedCheck = Checks.FirstOrDefault();
        }
    }

    private void Save() {
        _checksRepo.SetChecks(Checks.Select(c => c.GetCheck()).ToArray());
    }

    private void Export() {
        if(SaveFileDialogService.ShowDialog(
               _dirPath,
               _localization.GetLocalizedString("ChecksPage.SaveFileDefaultName"))) {
            var checks = Checks.Select(c => c.GetCheck()).ToArray();
            string str = _checksConverter.ConvertToString(checks);
            File.WriteAllText(SaveFileDialogService.File.FullName, str);
            _dirPath = SaveFileDialogService.File.DirectoryName;
        }
    }

    private bool CanSave() {
        return true; // TODO
    }

    private async Task SetSelectedFilesAsync(CheckViewModel check) {
        try {
            var selectedFiles = await _namesService.SelectNamesAsync(
                _localization.GetLocalizedString("ChecksPage.SetSelectedFilesPrompt"),
                _availableFiles.ToArray(),
                check.SelectedFiles.ToArray());
            check.SelectedFiles = [..selectedFiles];
        } catch(OperationCanceledException) {
        }
    }

    private async Task SetSelectedFiltersAsync(CheckViewModel check) {
        try {
            var selectedFilters = await _namesService.SelectNamesAsync(
                _localization.GetLocalizedString("ChecksPage.SetSelectedFiltersPrompt"),
                _availableFilters.ToArray(),
                check.SelectedFilters.ToArray());
            check.SelectedFilters = [..selectedFilters];
        } catch(OperationCanceledException) {
        }
    }

    private async Task SetSelectedRulesAsync(CheckViewModel check) {
        try {
            var selectedRules = await _namesService.SelectNamesAsync(
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
