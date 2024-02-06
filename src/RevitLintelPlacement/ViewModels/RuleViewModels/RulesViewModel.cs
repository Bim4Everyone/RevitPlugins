using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Interfaces;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.Models.RuleConfigManagers;
using RevitLintelPlacement.ViewModels.Services;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement.ViewModels.RuleViewModels {
    internal class RulesViewModel : BaseViewModel, INamedEntity {
        private readonly RevitRepository _revitRepository;
        private readonly RuleConfig _ruleConfig;
        private readonly ElementInfosViewModel _elementInfos;
        private string _name;
        private string _message;
        private DispatcherTimer _timer;
        private RuleConfigManager _ruleConfigManager;
        private ObservableCollection<GroupedRuleViewModel> _rules;

        public RulesViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos, RuleConfig ruleConfig) {
            _revitRepository = revitRepository;
            _elementInfos = elementInfos;
            _ruleConfig = ruleConfig;

            InitializeRules();
            InitializeTimer();

            SaveCommand = new RelayCommand(Save, CanSave);
            SaveAsCommand = new RelayCommand(SaveAs, CanSave);
            RenameCommand = new RelayCommand(Rename, CanRename);
            AddGroupedRuleCommand = new RelayCommand(AddGroupedRule);
            RemoveGroupedRuleCommand = new RelayCommand(RemoveGroupedRule, CanRemoveGroupedRule);
        }

        public bool IsLoaded { get; set; }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Message { 
            get => _message; 
            set => this.RaiseAndSetIfChanged(ref _message, value); 
        }

        public RuleConfig Config => GetUpdatedConfig();

        public RuleConfigManager RuleConfigManager {
            get => _ruleConfigManager;
            set => this.RaiseAndSetIfChanged(ref _ruleConfigManager, value);
        }

        public ObservableCollection<GroupedRuleViewModel> Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand AddGroupedRuleCommand { get; }
        public ICommand RemoveGroupedRuleCommand { get; }

        public static RulesViewModel GetLocalRules(RevitRepository revitRepository, ElementInfosViewModel elementInfos, RuleConfig ruleConfig) {
            return new RulesViewModel(revitRepository, elementInfos, ruleConfig) {
                RuleConfigManager = RuleConfigManager.GetLocalConfigManager()
            };
        }

        public static RulesViewModel GetProjectRules(RevitRepository revitRepository, ElementInfosViewModel elementInfos, RuleConfig ruleConfig) {
            return new RulesViewModel(revitRepository, elementInfos, ruleConfig) {
                RuleConfigManager = RuleConfigManager.GetProjectConfigManager()
            };
        }

        public static RulesViewModel GetTemplateRules(RevitRepository revitRepository, ElementInfosViewModel elementInfos, RuleConfig ruleConfig) {
            return new RulesViewModel(revitRepository, elementInfos, ruleConfig) {
                RuleConfigManager = RuleConfigManager.GetTemplateConfigManager()
            };
        }

        public static RulesViewModel GetLoadedRules(RevitRepository revitRepository, ElementInfosViewModel elementInfos, RuleConfig ruleConfig) {
            return new RulesViewModel(revitRepository, elementInfos, ruleConfig) {
                RuleConfigManager = RuleConfigManager.GetLocalConfigManager(),
                IsLoaded = true
            };
        }

        public string GetErrorText() {
            for(int i = 0; i < Rules.Count - 1; i++) {
                for(int j = i + 1; j < Rules.Count; j++) {
                    var commonWallTypes = Rules[i].WallTypes.WallTypes.Where(item=>item.IsChecked)
                    .Intersect(Rules[j].WallTypes.WallTypes.Where(item => item.IsChecked))
                    .ToList();
                    if(commonWallTypes.Count > 0) {
                        return $"У правил \"{Rules[i].Name}\" и \"{Rules[j].Name}\" " +
                            $"выбраны следующие одинаковые типоразмеры стен: \"{string.Join(", ", commonWallTypes.Select(e => e.Name))}\"";
                    }
                }
            }
            return string.Empty;
        }

        public ConcreteRuleViewModel GetRule(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            return Rules.Select(groupedRule => groupedRule.GetRule(familyInstance))
                .FirstOrDefault(rule => rule != null);
        }

        public RulesViewModel Copy(IEnumerable<RulesViewModel> rules) {
            if(RuleConfigManager.CanSave) {
                GetUpdatedConfig();
                Save();
            }

            var newConfig = RuleConfigManager.Copy(_ruleConfig, rules.Select(item => item.Config));

            var copiedRule = GetLocalRules(_revitRepository, _elementInfos, newConfig);
            copiedRule.Save();
            return copiedRule;
        }

        private void InitializeRules() {
            if(_ruleConfig.RuleSettings == null || _ruleConfig.RuleSettings.Count == 0) {
                Rules = new ObservableCollection<GroupedRuleViewModel>();
                Rules.Add(new GroupedRuleViewModel(_revitRepository, _elementInfos));
                Name = _ruleConfig.Name;
            } else {
                Rules = new ObservableCollection<GroupedRuleViewModel>(
                _ruleConfig.RuleSettings.Select(r => new GroupedRuleViewModel(_revitRepository, _elementInfos, r)));
                Name = _ruleConfig.Name;
            }
        }

        private void SaveWithQuestion() {
            if(GetResult() == TaskDialogResult.CommandLink2) {
                _ruleConfig.UpdateToLocalPath();
                IsLoaded = false;
            }

            Save();
        }

        private void Save() {
            GetUpdatedConfig();
            RuleConfigManager.Save(_ruleConfig);
        }

        private TaskDialogResult GetResult() {
            var dialog = new TaskDialog("BIM");
            dialog.MainContent = $"Выберите вариант сохранения файла.";
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Сохранить");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Сохранить локально");
            dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
            return dialog.Show();
        }

        private RuleConfig GetUpdatedConfig() {
            _ruleConfig.ProjectConfigPath = Path.Combine(Path.GetDirectoryName(_ruleConfig.ProjectConfigPath), Name + ".json");
            _ruleConfig.RuleSettings = Rules.Select(item => item.GetGroupedRuleSetting()).ToList();
            return _ruleConfig;
        }

        private void InitializeTimer() {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 3);
            _timer.Tick += (s, a) => { Message = null; _timer.Stop(); };
        }

        private void RefreshMessage() {
            _timer.Start();
        }

        private void Save(object p) {
            if(IsLoaded) {
                SaveWithQuestion();
                return;
            }
            Save();

            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private void SaveAs(object p) {
            GetUpdatedConfig();
            var saver = new ConfigSaverService();
            saver.Save(_ruleConfig);

            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private void Rename(object p) {
            var newRulesName = new RulesNameViewModel(((ObservableCollection<RulesViewModel>) p).Select(item => item.Name), Name);
            var view = new RulesNameView() { DataContext = newRulesName, Owner = p as Window };
            if(view.ShowDialog() == true) {
                Name = newRulesName.Name;
            }
            RuleConfigManager.Rename(_ruleConfig, Name);
        }

        private void AddGroupedRule(object p) {
            Rules.Add(new GroupedRuleViewModel(_revitRepository, _elementInfos));
        }

        private void RemoveGroupedRule(object p) {
            Rules.Remove((GroupedRuleViewModel) p);
        }

        private bool CanSave(object p) => RuleConfigManager.CanSave && string.IsNullOrEmpty(GetErrorText());

        private bool CanRemoveGroupedRule(object p) => p is GroupedRuleViewModel;

        private bool CanRename(object p) => RuleConfigManager.CanRename && p is ObservableCollection<RulesViewModel>;
    }
}