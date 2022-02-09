using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class ConfigViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly LintelsConfig _lintelsConfig;
        private LintelsCommonConfig _lintelsCommonConfig;
        private string _lintelThickness;
        private string _lintelWidth;
        private string _lintelRightOffset;
        private string _lintelLeftOffset;
        private string _lintelLeftCorner;
        private string _lintelRightCorner;
        private string _lintelFixation;
        private string _openingHeight;
        private string _openingWidth;
        private string _openingFixation;
        private ObservableCollection<FilterViewModel> _reinforcedConcreteFilter;
        private string _holesFilter;
        private string _lintelsConfigPath;
        private ObservableCollection<RulePathViewModel> _rulesCongigPath;
        private List<GenericModelFamilyViewModel> _lintelTypes;

        public ConfigViewModel() {
            RulesCongigPaths = new ObservableCollection<RulePathViewModel>();
        }

        public ConfigViewModel(RevitRepository revitRepository, LintelsConfig lintelsConfig, LintelsCommonConfig lintelsCommonConfig) {
            this._revitRepository = revitRepository;
            this._lintelsConfig = lintelsConfig;
            this._lintelsCommonConfig = lintelsCommonConfig;
            RulesCongigPaths = new ObservableCollection<RulePathViewModel>();
            LintelFamilies = new List<GenericModelFamilyViewModel>(revitRepository.GetGenericModelFamilies()
                .Select(f => new GenericModelFamilyViewModel() { Name = f.Name }));
            Initialize();
            SaveConfigCommand = new RelayCommand(Save, p => true);
            AddFilterCommand = new RelayCommand(AddFilter, p=>true);
            RemoveFilterCommand = new RelayCommand(RemoveFilter, p => true);
            AddRulePathCommand = new RelayCommand(AddRule, p => true);
            RemoveRulePathCommand = new RelayCommand(RemoveRule, p => true);
            SelectLintelsConfigCommand = new RelayCommand(SelectLintelsConfig, p=>true);
        }

        public string LintelThickness {
            get => _lintelThickness;
            set => this.RaiseAndSetIfChanged(ref _lintelThickness, value);
        }

        public string LintelWidth {
            get => _lintelWidth;
            set => this.RaiseAndSetIfChanged(ref _lintelWidth, value);
        }

        public string LintelRightOffset {
            get => _lintelRightOffset;
            set => this.RaiseAndSetIfChanged(ref _lintelRightOffset, value);
        }

        public string LintelLeftOffset {
            get => _lintelLeftOffset;
            set => this.RaiseAndSetIfChanged(ref _lintelLeftOffset, value);
        }

        public string LintelLeftCorner {
            get => _lintelLeftCorner;
            set => this.RaiseAndSetIfChanged(ref _lintelLeftCorner, value);
        }

        public string LintelRightCorner {
            get => _lintelRightCorner;
            set => this.RaiseAndSetIfChanged(ref _lintelRightCorner, value);
        }

        public string LintelFixation {
            get => _lintelFixation;
            set => this.RaiseAndSetIfChanged(ref _lintelFixation, value);
        }

        public string OpeningHeight {
            get => _openingHeight;
            set => this.RaiseAndSetIfChanged(ref _openingHeight, value);
        }

        public string OpeningWidth {
            get => _openingWidth;
            set => this.RaiseAndSetIfChanged(ref _openingWidth, value);
        }

        public string OpeningFixation {
            get => _openingFixation;
            set => this.RaiseAndSetIfChanged(ref _openingFixation, value);
        }

        public ObservableCollection<FilterViewModel> ReinforcedConcreteFilter {
            get => _reinforcedConcreteFilter;
            set => this.RaiseAndSetIfChanged(ref _reinforcedConcreteFilter, value);
        }
        public string HolesFilter {
            get => _holesFilter;
            set => this.RaiseAndSetIfChanged(ref _holesFilter, value);
        }

        public string LintelsConfigPath {
            get => _lintelsConfigPath;
            set => this.RaiseAndSetIfChanged(ref _lintelsConfigPath, value);
        }

        public ObservableCollection<RulePathViewModel> RulesCongigPaths {
            get => _rulesCongigPath;
            set => this.RaiseAndSetIfChanged(ref _rulesCongigPath, value);
        }

        public List<GenericModelFamilyViewModel> LintelFamilies { 
            get => _lintelTypes; 
            set => this.RaiseAndSetIfChanged(ref _lintelTypes, value); 
        }

        public ICommand SaveConfigCommand { get; set; }
        public ICommand AddFilterCommand { get; set; }
        public ICommand RemoveFilterCommand { get; set; }
        public ICommand AddRulePathCommand { get; set; }
        public ICommand RemoveRulePathCommand { get; set; }
        public ICommand SelectLintelsConfigCommand { get; set; }

        public void Initialize() {
            LintelThickness = _lintelsCommonConfig.LintelThickness;
            LintelWidth = _lintelsCommonConfig.LintelWidth;
            LintelRightCorner = _lintelsCommonConfig.LintelRightCorner;
            LintelRightOffset = _lintelsCommonConfig.LintelRightOffset;
            LintelLeftCorner = _lintelsCommonConfig.LintelLeftCorner;
            LintelLeftOffset = _lintelsCommonConfig.LintelLeftOffset;
            LintelFixation = _lintelsCommonConfig.LintelFixation;
            OpeningHeight = _lintelsCommonConfig.OpeningHeight;
            OpeningWidth = _lintelsCommonConfig.OpeningWidth;
            OpeningFixation = _lintelsCommonConfig.OpeningFixation;
            if (_lintelsCommonConfig.ReinforcedConcreteFilter==null || _lintelsCommonConfig.ReinforcedConcreteFilter.Count == 0) {
                ReinforcedConcreteFilter = new ObservableCollection<FilterViewModel>(
                new List<FilterViewModel> { new FilterViewModel() { Name = "Железобетон" } });
            } else {
                ReinforcedConcreteFilter = new ObservableCollection<FilterViewModel>(
                new List<FilterViewModel>(_lintelsCommonConfig.ReinforcedConcreteFilter
                .Select(e => new FilterViewModel() { Name = e })));
            }
            
            HolesFilter = _lintelsCommonConfig.HolesFilter;
            LintelsConfigPath = _lintelsConfig.LintelsConfigPath;
            if (_lintelsConfig.RulesCongigPaths!=null && _lintelsConfig.RulesCongigPaths.Count > 0) {
                RulesCongigPaths = new ObservableCollection<RulePathViewModel>(
                    _lintelsConfig.RulesCongigPaths.Select(p => new RulePathViewModel() { Name = p }));
            }
            foreach(var family in LintelFamilies) {
                family.IsChecked = _lintelsCommonConfig.LintelFamilies
                    .Any(f => f.Equals(family.Name, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        private void Save(object p) {
            _lintelsCommonConfig.LintelThickness = LintelThickness;
            _lintelsCommonConfig.LintelWidth = LintelWidth;
            _lintelsCommonConfig.LintelRightCorner = LintelRightCorner;
            _lintelsCommonConfig.LintelRightOffset = LintelRightOffset;
            _lintelsCommonConfig.LintelLeftCorner = LintelLeftCorner;
            _lintelsCommonConfig.LintelLeftOffset = LintelLeftOffset;
            _lintelsCommonConfig.LintelFixation = LintelFixation;
            _lintelsCommonConfig.OpeningHeight = OpeningHeight;
            _lintelsCommonConfig.OpeningWidth = OpeningWidth;
            _lintelsCommonConfig.OpeningFixation = OpeningFixation;
            _lintelsCommonConfig.ReinforcedConcreteFilter = ReinforcedConcreteFilter.Select(e=>e.Name).ToList();
            _lintelsCommonConfig.HolesFilter = HolesFilter;
            _lintelsConfig.LintelsConfigPath = LintelsConfigPath;
            _lintelsConfig.RulesCongigPaths = RulesCongigPaths.Select(e=>e.Name).ToList();
            _lintelsCommonConfig.LintelFamilies = LintelFamilies
                .Where(e => e.IsChecked)
                .Select(e => e.Name)
                .ToList();
            _lintelsCommonConfig.Save(_lintelsConfig.LintelsConfigPath);
            _lintelsConfig.SaveProjectConfig();
        }

        private void AddFilter(object p) {
            ReinforcedConcreteFilter.Add(new FilterViewModel());
        }

        private void RemoveFilter(object p) {
            if(ReinforcedConcreteFilter.Count > 0) {
                ReinforcedConcreteFilter.Remove(ReinforcedConcreteFilter.Last());
            }
        }
        private void AddRule(object p) {
            RulesCongigPaths.Add(new RulePathViewModel());
        }

        private void RemoveRule(object p) {
            if(RulesCongigPaths.Count > 0) {
                RulesCongigPaths.Remove(RulesCongigPaths.Last());
            }
        }

        private void SelectLintelsConfig(object p) {
            using(var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.SelectedPath = System.IO.Path.GetDirectoryName(LintelsConfigPath);
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    if(dialog.SelectedPath!= LintelsConfigPath) {
                        LintelsConfigPath = dialog.SelectedPath;
                        _lintelsCommonConfig = LintelsCommonConfig.GetLintelsCommonConfig(LintelsConfigPath);
                        _lintelsConfig.LintelsConfigPath = LintelsConfigPath;
                        Initialize();
                    }
                }

            }
        }

    }

    internal class FilterViewModel : BaseViewModel {
        private string _name;

        public string Name { 
            get => _name; 
            set => this.RaiseAndSetIfChanged(ref _name, value); 
        }
    }

    internal class RulePathViewModel : BaseViewModel {
        private string _name;

        public RulePathViewModel() {
            SelectPathCommand = new RelayCommand(SelectRulePath, p => true);
        }

        public string Name { 
            get => _name; 
            set => this.RaiseAndSetIfChanged(ref _name, value); 
        }
        public ICommand SelectPathCommand { get; set; }

        private void SelectRulePath(object p) {
            using(var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.SelectedPath = System.IO.Path.GetDirectoryName(Name);
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    if(dialog.SelectedPath != Name) {
                        Name = dialog.SelectedPath;
                    }
                }

            }
        }
    }
}