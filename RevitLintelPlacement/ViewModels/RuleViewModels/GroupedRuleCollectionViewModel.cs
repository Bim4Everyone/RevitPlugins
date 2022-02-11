using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<GroupedRuleViewModel> _groupedRules;
        private ObservableCollection<string> _rulePaths;
        private string _selectedPath;

        public GroupedRuleCollectionViewModel() {

        }

        public GroupedRuleCollectionViewModel(RevitRepository revitRepository, LintelsConfig lintelsConfig, IEnumerable<GroupedRuleSettings> rules = null) {
            AddGroupedRuleCommand = new RelayCommand(AddGroupedRule, p => true);
            RemoveGroupedRuleCommand = new RelayCommand(RemoveGroupedRule, p => true);
            SaveCommand = new RelayCommand(Save, p => true);
            SaveAsCommand = new RelayCommand(SaveAs, p => true);
            LoadCommand = new RelayCommand(Load, p => true);
            PathSelectionChangedCommand = new RelayCommand(SelectionChanged, p => true);
            this._revitRepository = revitRepository;
            InitializeGroupRules(rules);
            if(lintelsConfig.RulesCongigPaths == null || lintelsConfig.RulesCongigPaths.Count == 0) {
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                RulePaths = new ObservableCollection<string>(new Collection<string> { SelectedPath });

            } else {
                RulePaths = new ObservableCollection<string>(lintelsConfig.RulesCongigPaths);
                SelectedPath = RulePaths.FirstOrDefault();
            }
        }

        public ICommand AddGroupedRuleCommand { get; set; }
        public ICommand RemoveGroupedRuleCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand PathSelectionChangedCommand { get; set; }

        public ObservableCollection<GroupedRuleViewModel> GroupedRules {
            get => _groupedRules;
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        public ObservableCollection<string> RulePaths {
            get => _rulePaths;
            set => this.RaiseAndSetIfChanged(ref _rulePaths, value);
        }

        public string SelectedPath {
            get => _selectedPath;
            set => this.RaiseAndSetIfChanged(ref _selectedPath, value);
        }

        public ConcreteRuleViewModel GetRule(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            return GroupedRules.Select(groupedRule => groupedRule.GetRule(familyInstance))
                .FirstOrDefault(rule => rule != null);
        }

        public void Save(object p) {
            RuleConfig config;
            if(string.IsNullOrEmpty(SelectedPath)) {
                config = RuleConfig.GetRuleConfig();
            } else {
                config = RuleConfig.GetRuleConfig(SelectedPath);
            }
            config.RuleSettings = new List<GroupedRuleSettings>();
            foreach(var rule in GroupedRules) {
                config.RuleSettings.Add(rule.GetGroupedRuleSetting());
            }
            config.Save(SelectedPath);
        }


        private void SaveAs(object p) {
            using(var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.SelectedPath = System.IO.Path.GetDirectoryName(SelectedPath);
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    SelectedPath = dialog.SelectedPath;
                    Save(p);
                }
            }
        }

        private void Load(object p) {
            using(OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.InitialDirectory = SelectedPath;
                openFileDialog.Filter = "Json files (*.json)|*.json";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if(openFileDialog.ShowDialog() == DialogResult.OK) {
                    var filePath = openFileDialog.FileName;
                    RuleConfig config;
                    try {
                        config = RuleConfig.GetRuleFromFile(filePath);
                    } catch {
                        config = RuleConfig.GetRuleConfig(SelectedPath);
                    }
                    InitializeGroupRules(config.RuleSettings);
                }
            }
        }

        private void AddGroupedRule(object p) {
            GroupedRules.Add(new GroupedRuleViewModel(_revitRepository));
        }

        private void InitializeGroupRules(IEnumerable<GroupedRuleSettings> rules) {
            if(rules == null || rules.Count() == 0) {
                GroupedRules = new ObservableCollection<GroupedRuleViewModel>();
                GroupedRules.Add(new GroupedRuleViewModel(_revitRepository));
            } else {
                GroupedRules = new ObservableCollection<GroupedRuleViewModel>(
                rules.Select(r => new GroupedRuleViewModel(_revitRepository, r))
                );
            }
        }

        private void RemoveGroupedRule(object p) {
            if(p is GroupedRuleViewModel rule) {
                GroupedRules.Remove(rule);
            }
        }

        private void SelectionChanged(object p) {
            var config = RuleConfig.GetRuleConfig(SelectedPath);
            InitializeGroupRules(config.RuleSettings);
        }
    }
}
