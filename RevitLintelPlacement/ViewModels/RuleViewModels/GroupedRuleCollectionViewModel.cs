using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.Models.RuleConfigManagers;
using RevitLintelPlacement.ViewModels.RuleViewModels;
using RevitLintelPlacement.ViewModels.Services;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement.ViewModels {
    internal class GroupedRuleCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private string _message;
        private ObservableCollection<RulesViewModel> _rules;
        private RulesViewModel _selectedRule;

        public GroupedRuleCollectionViewModel() {

        }

        public GroupedRuleCollectionViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            _revitRepository = revitRepository;
            _elementInfos = elementInfos;

            InitializeGroupRules();

            SelectedRule = Rules.FirstOrDefault(item => item.Name.Equals(_revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName())?.SelectedPath,
                                                                         StringComparison.CurrentCulture));
            if(SelectedRule == null) {
                SelectedRule = Rules.FirstOrDefault();
            }

            CopyCommand = new RelayCommand(Copy);
            LoadCommand = new RelayCommand(Load);
            CreateNewRuleCommand = new RelayCommand(CreateNewRule);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
        }

        public RulesViewModel SelectedRule {
            get => _selectedRule;
            set => this.RaiseAndSetIfChanged(ref _selectedRule, value);
        }

        public ICommand CopyCommand { get; }
        public ICommand LoadCommand { get; set; }
        public ICommand CreateNewRuleCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ObservableCollection<RulesViewModel> Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public ConcreteRuleViewModel GetRule(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            return SelectedRule.GetRule(familyInstance);
        }

        private void InitializeGroupRules() {
            var rules = new List<RulesViewModel>();
            var templateRules = GetTemplateRules();
            if(templateRules != null) {
                rules.Add(templateRules);
            }
            var projectRules = GetProjectRules();
            if(projectRules != null) {
                rules.Add(projectRules);
            }
            rules.AddRange(GetLocalRules());
            Rules = new ObservableCollection<RulesViewModel>(rules);
        }

        private RulesViewModel GetTemplateRules() {
            var templatePath = _revitRepository.GetTemplatePath();
            if(File.Exists(templatePath)) {
                var config = RuleConfig.GetRuleConfigs(templatePath);
                return RulesViewModel.GetTemplateRules(_revitRepository, _elementInfos, config);
            }
            return null;
        }

        private RulesViewModel GetProjectRules() {
            var projectPath = _revitRepository.GetProjectPath();
            if(File.Exists(projectPath)) {
                var config = RuleConfig.GetRuleConfigs(projectPath);
                return RulesViewModel.GetProjectRules(_revitRepository, _elementInfos, config);
            } else if(RevitRepository.HasEmptyProjectPath()) {
                var config = RuleConfig.GetEmptyProjectConfig(projectPath);
                var templateRules = GetTemplateRules();
                if(templateRules != null) {
                    config.RuleSettings = templateRules.Config.RuleSettings;
                }
                return RulesViewModel.GetProjectRules(_revitRepository, _elementInfos, config);
            }
            return null;
        }

        private IEnumerable<RulesViewModel> GetLocalRules() {
            var projectPath = RevitRepository.LocalRulePath;
            if(projectPath != null && Directory.Exists(projectPath)) {
                var configPaths = Directory.GetFiles(projectPath, "*.json");
                foreach(var configPath in configPaths) {
                    var config = RuleConfig.GetRuleConfigs(configPath);
                    yield return RulesViewModel.GetLocalRules(_revitRepository, _elementInfos, config);
                }
            }
        }

        private void Copy(object p) {
            var newRule = SelectedRule.Copy(Rules);
            Rules.Add(newRule);
            Rules = new ObservableCollection<RulesViewModel>(Rules.OrderBy(item => item.Name));
            SelectedRule = newRule;
        }

        private void CreateNewRule(object p) {
            var newRulesName = new RulesNameViewModel(Rules.Select(item => item.Name));
            var view = new RulesNameView() { DataContext = newRulesName, Owner = p as Window };
            if(view.ShowDialog() == true) {
                var newRulesConfig = RuleConfig.GetLocalRuleConfig(newRulesName.Name);
                var newRules = RulesViewModel.GetLocalRules(_revitRepository, _elementInfos, newRulesConfig);
                newRules.Name = newRulesName.Name;
                Rules.Add(newRules);
                Rules = new ObservableCollection<RulesViewModel>(Rules.OrderBy(item => item.Name));
                SelectedRule = newRules;
            }
        }

        private void Load(object p) {
            var loader = new ConfigLoaderService();
            var config = loader.Load<RuleConfig>();
            var newRules = RulesViewModel.GetLoadedRules(_revitRepository, _elementInfos, config);
            var nameResolver = new NameResolver<RulesViewModel>(Rules, new[] { newRules });
            Rules = new ObservableCollection<RulesViewModel>(nameResolver.GetCollection());
            SelectedRule = Rules.FirstOrDefault(item => item.Name.Equals(newRules.Name, StringComparison.CurrentCulture));
        }

        private void Delete(object p) {
            var mb = GetPlatformService<IMessageBoxService>();
            if(mb.Show("Вы уверены, что хотите удалить файл?", "BIM", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Yes) {
                SelectedRule.RuleConfigManager.Delete(SelectedRule.Config);
                Rules.Remove(SelectedRule);
                SelectedRule = Rules.FirstOrDefault();
            }
        }

        private bool CanDelete(object p) => SelectedRule!=null && SelectedRule.RuleConfigManager.CanDelete;
    }
}