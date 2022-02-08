using System;
using System.Collections.Generic;
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
        private readonly LintelsCommonConfig _lintelsCommonConfig;
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
        private string _reinforcedConcreteFilter;
        private string _holesFilter;
        private string _lintelsConfigPath;
        private List<string> _rulesCongigPath = new List<string>();
        private List<GenericModelFamilyViewModel> _lintelTypes;

        public ConfigViewModel() {
            RulesCongigPaths = new List<string>();
        }

        public ConfigViewModel(RevitRepository revitRepository, LintelsConfig lintelsConfig, LintelsCommonConfig lintelsCommonConfig) {
            this._revitRepository = revitRepository;
            this._lintelsConfig = lintelsConfig;
            this._lintelsCommonConfig = lintelsCommonConfig;
            RulesCongigPaths = new List<string>();
            LintelFamilies = new List<GenericModelFamilyViewModel>(revitRepository.GetGenericModelFamilies()
                .Select(f => new GenericModelFamilyViewModel() { Name = f.Name }));
            Initialize(lintelsConfig, lintelsCommonConfig);
            SaveConfigCommand = new RelayCommand(Save, p => true);
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

        public string ReinforcedConcreteFilter {
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

        public List<string> RulesCongigPaths {
            get => _rulesCongigPath;
            set => this.RaiseAndSetIfChanged(ref _rulesCongigPath, value);
        }

        public List<GenericModelFamilyViewModel> LintelFamilies { 
            get => _lintelTypes; 
            set => this.RaiseAndSetIfChanged(ref _lintelTypes, value); 
        }

        public ICommand SaveConfigCommand { get; set; }

        public void Initialize(LintelsConfig lintelsConfig, LintelsCommonConfig lintelsCommonConfig) {
            LintelThickness = lintelsCommonConfig.LintelThickness;
            LintelWidth = lintelsCommonConfig.LintelWidth;
            LintelRightCorner = lintelsCommonConfig.LintelRightCorner;
            LintelRightOffset = lintelsCommonConfig.LintelRightOffset;
            LintelLeftCorner = lintelsCommonConfig.LintelLeftCorner;
            LintelLeftOffset = lintelsCommonConfig.LintelLeftOffset;
            LintelFixation = lintelsCommonConfig.LintelFixation;
            OpeningHeight = lintelsCommonConfig.OpeningHeight;
            OpeningWidth = lintelsCommonConfig.OpeningWidth;
            OpeningFixation = lintelsCommonConfig.OpeningFixation;
            ReinforcedConcreteFilter = lintelsCommonConfig.ReinforcedConcreteFilter;
            HolesFilter = lintelsCommonConfig.HolesFilter;
            LintelsConfigPath = lintelsConfig.LintelsConfigPath;
            RulesCongigPaths = lintelsConfig.RulesCongigPaths;
            foreach(var family in LintelFamilies) {
                family.IsChecked = lintelsCommonConfig.LintelFamilies
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
            _lintelsCommonConfig.ReinforcedConcreteFilter = ReinforcedConcreteFilter;
            _lintelsCommonConfig.HolesFilter = HolesFilter;
            _lintelsConfig.LintelsConfigPath = LintelsConfigPath;
            _lintelsConfig.RulesCongigPaths = RulesCongigPaths;
            _lintelsCommonConfig.LintelFamilies = LintelFamilies
                .Where(e => e.IsChecked)
                .Select(e => e.Name)
                .ToList();
            _lintelsCommonConfig.Save(_lintelsConfig.LintelsConfigPath);
            _lintelsConfig.SaveProjectConfig();
        }
    }
}
