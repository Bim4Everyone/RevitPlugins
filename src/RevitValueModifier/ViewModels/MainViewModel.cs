using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
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
        private RevitParameter _selectedCommonParam;
        private RevitParameter _selectedCommonParamForAdd;
        private List<RevitElement> _revitElements;
        private List<RevitParameter> _commonParams;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            AddParamInMaskCommand = RelayCommand.Create(AddParamInMask);
            ParamUpdateCommand = RelayCommand.Create(ParamUpdate);

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand AddParamInMaskCommand { get; }
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

        public RevitParameter SelectedCommonParam {
            get => _selectedCommonParam;
            set => this.RaiseAndSetIfChanged(ref _selectedCommonParam, value);
        }

        public RevitParameter SelectedCommonParamForAdd {
            get => _selectedCommonParamForAdd;
            set => this.RaiseAndSetIfChanged(ref _selectedCommonParamForAdd, value);
        }

        private void LoadView() {
            RevitElements = _revitRepository.GetRevitElements();
            List<ElementId> categoryIds = _revitRepository.GetCategoryIds(RevitElements);
            CommonParams = _revitRepository.GetParams(categoryIds);

            LoadConfig();
            ParamUpdateCommand.Execute(null);
        }
        private void AcceptView() {
            using(Transaction transaction = _revitRepository.Document.StartTransaction("Изменение значений параметров")) {
                foreach(RevitElement revitElement in RevitElements) {
                    revitElement.WriteParamValue(SelectedCommonParam);
                }
                transaction.Commit();
            }
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(ParamValueMask)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ParamValueMaskEmpty");
                return false;
            }

            if(SelectedCommonParam is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.SelectParamToRecord");
                return false;
            }

            //StorageType storageType;
            //if(SelectedCommonParam.IsBuiltin) {
            //    storageType = _revitRepository.Document.get_TypeOfStorage(SelectedCommonParam.BInParameter);
            //} else {
            //    storageType = SelectedCommonParam.ParamElement.GetStorageType();
            //}

            //if(storageType == StorageType.Integer) {
            //    if(!int.TryParse(ParamValueMask, out _)) {
            //        ErrorText = _localizationService.GetLocalizedString("MainWindow.FailIntParseParamValueMask");
            //    }
            //} else if (storageType == StorageType.Double) {
            //    if(!double.TryParse(ParamValueMask, out _)) {
            //        ErrorText = _localizationService.GetLocalizedString("MainWindow.FailDoubleParseParamValueMask");
            //    }
            //}

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            ParamValueMask = setting?.TaskForWrite ?? _localizationService.GetLocalizedString("MainWindow.ParamValueMask");
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

        private void AddParamInMask() {
            ParamValueMask += $"{{{SelectedCommonParamForAdd.ParamName}}}";
        }
    }
}
