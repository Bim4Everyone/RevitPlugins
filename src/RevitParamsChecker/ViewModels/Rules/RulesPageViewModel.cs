using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Rules;

internal class RulesPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly RulesRepository _rulesRepo;
    private readonly RulesConverter _rulesConverter;
    private readonly NamesService _namesService;
    private RuleViewModel _selectedRule;
    private string _dirPath;

    public RulesPageViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        RulesRepository rulesRepo,
        RulesConverter rulesConverter,
        NamesService namesService) {
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _rulesRepo = rulesRepo ?? throw new ArgumentNullException(nameof(rulesRepo));
        _rulesConverter = rulesConverter ?? throw new ArgumentNullException(nameof(rulesConverter));
        _namesService = namesService ?? throw new ArgumentNullException(nameof(namesService));
        _dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        Rules = [
            .._rulesRepo.GetRules()
                .Select(r => new RuleViewModel(r, _localization))
        ];
        SelectedRule = Rules.FirstOrDefault();
        AddRuleCommand = RelayCommand.Create(AddRule);
        RenameRuleCommand = RelayCommand.Create<RuleViewModel>(RenameRule, CanRenameRule);
        RemoveRulesCommand = RelayCommand.Create<IList>(RemoveRules, CanRemoveRules);
        CopyRuleCommand = RelayCommand.Create<RuleViewModel>(CopyRule, CanCopyRule);
        LoadCommand = RelayCommand.Create(Load);
        SaveCommand = RelayCommand.Create(Save, CanSave);
        ExportCommand = RelayCommand.Create(Export, CanSave);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand AddRuleCommand { get; }
    public ICommand RenameRuleCommand { get; }
    public ICommand CopyRuleCommand { get; }
    public ICommand RemoveRulesCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

    public ObservableCollection<RuleViewModel> Rules { get; }

    public RuleViewModel SelectedRule {
        get => _selectedRule;
        set => RaiseAndSetIfChanged(ref _selectedRule, value);
    }

    private void AddRule() {
        try {
            var newRule = new Rule();
            newRule.Name = _namesService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.NewRulePrompt"),
                Rules.Select(f => f.Name).ToArray());
            var vm = new RuleViewModel(newRule, _localization) { Modified = true };
            Rules.Add(vm);
            SelectedRule = vm;
        } catch(OperationCanceledException) {
        }
    }

    private void RenameRule(RuleViewModel rule) {
        try {
            rule.Name = _namesService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.RenameRulePrompt"),
                Rules.Select(f => f.Name).ToArray(),
                rule.Name);
            rule.Modified = true;
        } catch(OperationCanceledException) {
        }
    }

    private void CopyRule(RuleViewModel rule) {
        try {
            var copyRule = rule.GetRule().Copy();
            copyRule.Name = _namesService.CreateNewName(
                _localization.GetLocalizedString("RulesPage.NewRulePrompt"),
                Rules.Select(f => f.Name).ToArray(),
                rule.Name);
            var vm = new RuleViewModel(copyRule, _localization) { Modified = true };
            Rules.Add(vm);
            SelectedRule = vm;
        } catch(OperationCanceledException) {
        }
    }

    private bool CanCopyRule(RuleViewModel rule) {
        return rule != null; // TODO валидация правила
    }

    private bool CanRenameRule(RuleViewModel rule) {
        return rule is not null;
    }

    private void RemoveRules(IList items) {
        var rules = items.OfType<RuleViewModel>().ToArray();
        foreach(var rule in rules) {
            Rules.Remove(rule);
        }

        SelectedRule = Rules.FirstOrDefault();
    }

    private bool CanRemoveRules(IList items) {
        return items != null
               && items.OfType<RuleViewModel>().Count() != 0;
    }

    private void Load() {
        if(OpenFileDialogService.ShowDialog(_dirPath)) {
            string str = File.ReadAllText(OpenFileDialogService.File.FullName);
            Rule[] rules;
            try {
                rules = _rulesConverter.ConvertFromString(str);
            } catch(InvalidOperationException) {
                MessageBoxService.Show(_localization.GetLocalizedString("RulesPage.Error.CannotLoadRules"));
                return;
            }

            var vms = _namesService.GetResolvedCollection(
                    Rules.ToArray(),
                    rules.Select(r => new RuleViewModel(r, _localization)).ToArray())
                .OfType<RuleViewModel>();
            string selectedName = SelectedRule.Name;
            Rules.Clear();
            foreach(var vm in vms) {
                vm.Modified = true;
                Rules.Add(vm);
            }

            SelectedRule = Rules.FirstOrDefault(r => r.Name.Equals(selectedName)) ?? Rules.FirstOrDefault();
            _dirPath = OpenFileDialogService.File.DirectoryName;
        }
    }

    private void Save() {
        _rulesRepo.SetRules(Rules.Select(r => r.GetRule()).ToArray());
        foreach(var vm in Rules) {
            vm.Modified = false;
        }
    }

    private void Export() {
        if(SaveFileDialogService.ShowDialog(
               _dirPath,
               _localization.GetLocalizedString("RulesPage.SaveFileDefaultName"))) {
            var rules = Rules.Select(r => r.GetRule()).ToArray();
            string str = _rulesConverter.ConvertToString(rules);
            File.WriteAllText(SaveFileDialogService.File.FullName, str);
            _dirPath = SaveFileDialogService.File.DirectoryName;
        }
    }

    private bool CanSave() {
        return true; // TODO валидация
    }
}
