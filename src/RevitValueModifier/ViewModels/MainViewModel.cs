using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitValueModifier.Models;

namespace RevitValueModifier.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _paramValueMask;
        private List<RevitElement> _revitElements;
        private List<RevitParameter> _commonParams;

        //private RevitElemUtils _elemHelper;
        //private TaskParser _parserForTask;
        //private List<RevitParameter> _intersectedParametersNotReadOnly;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            ParamUpdateCommand = RelayCommand.Create(ParamUpdate);

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand ParamUpdateCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string ParamValueMask {
            get => _paramValueMask;
            set => this.RaiseAndSetIfChanged(ref _paramValueMask, value);
        }

        public List<RevitElement> RevitElements {
            get => _revitElements;
            set => this.RaiseAndSetIfChanged(ref _revitElements, value);
        }

        public List<RevitParameter> CommonParams {
            get => _commonParams;
            set => this.RaiseAndSetIfChanged(ref _commonParams, value);
        }

        private void LoadView() {
            RevitElements = _revitRepository.GetRevitElements();
            List<ElementId> categoryIds = _revitRepository.GetCategoryIds(RevitElements);
            CommonParams = _revitRepository.GetParams(categoryIds);

            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(ParamValueMask)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            ParamValueMask = setting?.TaskForWrite ?? _localizationService.GetLocalizedString("MainWindow.Hello");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.TaskForWrite = ParamValueMask;
            _pluginConfig.SaveProjectConfig();
        }

        private void ParamUpdate() {
            foreach(RevitElement revitElement in RevitElements) {
                revitElement.SetParamValue(ParamValueMask);
            }
        }
    }
}
