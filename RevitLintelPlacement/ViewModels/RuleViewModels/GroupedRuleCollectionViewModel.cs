using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private ObservableCollection<GroupedRuleViewModel> _groupedRules;
        private ObservableCollection<string> _ruleNames;
        private string _selectedName;
        private string _message;

        public GroupedRuleCollectionViewModel() {

        }

        public GroupedRuleCollectionViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            this._revitRepository = revitRepository;
            this._elementInfos = elementInfos;
            InitializeRulePaths();
            InitializeGroupRules();
            AddGroupedRuleCommand = new RelayCommand(AddGroupedRule, p => true);
            RemoveGroupedRuleCommand = new RelayCommand(RemoveGroupedRule, p => true);
            SaveCommand = new RelayCommand(Save, p => true);
            SaveAsCommand = new RelayCommand(SaveAs, p => true);
            LoadCommand = new RelayCommand(Load, p => true);
            PathSelectionChangedCommand = new RelayCommand(SelectionChanged, p => true);
        }

        public ICommand AddGroupedRuleCommand { get; set; }
        public ICommand RemoveGroupedRuleCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand PathSelectionChangedCommand { get; set; }
       

        public string Message { 
            get => _message; 
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public ObservableCollection<GroupedRuleViewModel> GroupedRules {
            get => _groupedRules;
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        public ObservableCollection<string> RuleNames {
            get => _ruleNames;
            set => this.RaiseAndSetIfChanged(ref _ruleNames, value);
        }

        public string Name { get; set; }

        public string SelectedName {
            get => _selectedName;
            set => this.RaiseAndSetIfChanged(ref _selectedName, value);
        }

        public ConcreteRuleViewModel GetRule(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            return GroupedRules.Select(groupedRule => groupedRule.GetRule(familyInstance))
                .FirstOrDefault(rule => rule != null);
        }

        public void Save(object p) {
            var config = GenerateConfig();
            if(config != null) {
                switch(config.RulesType) {
                    case RulesType.Common: {
                        config.Save(_revitRepository.GetDocumentName());
                        ChangeMessage("Файл успешно сохранен");
                        break;
                    }
                    case RulesType.Project: {
                        config.Save(_revitRepository.GetDocumentName());
                        ChangeMessage("Файл успешно сохранен");
                        break;
                    }
                    case RulesType.User: {
                        config.SaveAs(config.Name);
                        ChangeMessage("Файл успешно сохранен");
                        break;
                    }
                    default: {
                        throw new ArgumentNullException(nameof(config.RulesType), $"Следуюший тип правила \"{config.RulesType}\" не найден");
                    }
                }
            }
        }

        private void SaveAs(object p) {
            var vm = new SaveAsViewModel();
            var saveAsWindow = new SaveAsWindow() { DataContext = vm };
            saveAsWindow.ShowDialog();
            if(vm.RulesFileName != null) {
                var config = GenerateConfig();
                if(config != null) {
                    config.SaveAs(vm.RulesFileName);
                    if(!_revitRepository.RuleConfigs.ContainsKey(config.Name)) {
                        _revitRepository.RuleConfigs.Add(config.Name, config);
                        RuleNames.Add(config.Name);
                        ChangeMessage("Файл успешно сохранен");
                    }
                }
            }
        }

        private RuleConfig GenerateConfig() {
            RuleConfig config;
            if(!_revitRepository.RuleConfigs.ContainsKey(Name)) {
                throw new ArgumentException(nameof(_revitRepository.RuleConfigs), $"Не загружены правила со следующим имененм\"{Name}\".");
            }
            config = _revitRepository.RuleConfigs[Name];
            if(GroupedRules == null || GroupedRules.Count == 0) {
                config.RuleSettings = new List<GroupedRuleSettings>();
            } else {
                var newRules = new List<GroupedRuleSettings>();
                foreach(var rule in GroupedRules) {
                    var newRule = rule.GetGroupedRuleSetting();
                    var oldRule = config.RuleSettings.FirstOrDefault(r => r.Name != null && r.Name.Equals(rule.Name, StringComparison.CurrentCultureIgnoreCase));
                    if(oldRule == null) {
                        newRules.Add(newRule);
                        continue;
                    }
                    var absentWalls = oldRule.WallTypes.WallTypes.Except(rule.WallTypes.WallTypes.Select(w => w.Name)).ToList();
                    if(absentWalls.Count > 0) {
                        newRule.WallTypes.WallTypes.AddRange(absentWalls);
                    }
                    newRules.Add(newRule);
                }
                config.RuleSettings = newRules;
            }
            return config;
        }

        private void Load(object p) {
            using(OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "Json files (*.json)|*.json";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if(openFileDialog.ShowDialog() == DialogResult.OK) {
                    var filePath = openFileDialog.FileName;
                    RuleConfig config = RuleConfig.LoadConfigFromFile(filePath);
                    if(_revitRepository.RuleConfigs.ContainsKey(config.Name)) {
                        _revitRepository.RuleConfigs[config.Name] = config;
                    } else {
                        _revitRepository.RuleConfigs.Add(config.Name, config);
                    }
                    InitializeRulePaths();
                    SelectedName = config.Name;
                    InitializeGroupRules();
                    Save(null);
                }
            }
        }

        private void AddGroupedRule(object p) {
            GroupedRules.Add(new GroupedRuleViewModel(_revitRepository, _elementInfos));
        }

        private void InitializeGroupRules() {
            if(SelectedName != null && _revitRepository.RuleConfigs.ContainsKey(SelectedName)) {
                Name = _revitRepository.RuleConfigs[SelectedName].Name;
                InitializeGroupRules(_revitRepository.RuleConfigs[SelectedName].RuleSettings);
            } else {
                SelectedName = _revitRepository.GetDocumentName();
                if(!_revitRepository.RuleConfigs.ContainsKey(SelectedName)) {
                    throw new ArgumentNullException(nameof(SelectedName), $"Не загружены правила для проекта.");
                }
                Name = _revitRepository.RuleConfigs[SelectedName].Name;
                InitializeGroupRules(_revitRepository.RuleConfigs[SelectedName].RuleSettings);
            }
        }

        private void InitializeGroupRules(IEnumerable<GroupedRuleSettings> rules) {
            if(rules == null || rules.Count() == 0) {
                GroupedRules = new ObservableCollection<GroupedRuleViewModel>();
                GroupedRules.Add(new GroupedRuleViewModel(_revitRepository, _elementInfos));
            } else {
                GroupedRules = new ObservableCollection<GroupedRuleViewModel>(
                rules.Select(r => new GroupedRuleViewModel(_revitRepository, _elementInfos, r))
                );
            }
        }

        private void RemoveGroupedRule(object p) {
            if(p is GroupedRuleViewModel rule) {
                GroupedRules.Remove(rule);
            }
        }

        private void InitializeRulePaths() {
            if(_revitRepository.RuleConfigs?.Keys == null && _revitRepository.RuleConfigs?.Keys.Count() == 0) {
                throw new ArgumentException(nameof(_revitRepository.RuleConfigs), "Нет загруженных правил.");
            }
            RuleNames = new ObservableCollection<string>(_revitRepository.RuleConfigs.Keys);
            var settings = _revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName());
            if(settings != null && !string.IsNullOrEmpty(settings.SelectedPath) && RuleNames.Contains(settings.SelectedPath)) {
                SelectedName = settings.SelectedPath;
            } else {
                SelectedName = _revitRepository.GetDocumentName();
            }
        }

        private void SelectionChanged(object p) {
            var changedConfig = GenerateConfig();
            if(changedConfig.Name != null && _revitRepository.RuleConfigs.ContainsKey(changedConfig.Name)) {
                _revitRepository.RuleConfigs[changedConfig.Name] = changedConfig;
            }
            var config = _revitRepository.RuleConfigs[SelectedName];
            InitializeGroupRules();
        }

        private async void ChangeMessage(string newMessage) {
            Message = newMessage;
            await Task.Run(() => {
                Thread.Sleep(3000);
            });
            Message = string.Empty;
        }
    }
}