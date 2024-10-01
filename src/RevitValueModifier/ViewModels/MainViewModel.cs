using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

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

        private DispatcherTimer _timerByChangedMask;

        private string _errorText;
        private string _paramValueMask;
        private int _paramValueMaskCaretIndex = 0;

        private ICollectionView _revitElementsView;
        private string _revitElementsFilter = string.Empty;

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

            CreateDispatcherTimer();

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

        public int ParamValueMaskCaretIndex {
            get => _paramValueMaskCaretIndex;
            set => this.RaiseAndSetIfChanged(ref _paramValueMaskCaretIndex, value);
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

        /// <summary>
        /// Текстовое поле для привязки к TextBlock GUI фильтра списка элементов
        /// </summary>
        public string RevitElementsFilter {
            get => _revitElementsFilter;
            set {
                if(value != _revitElementsFilter) {
                    _revitElementsFilter = value;
                    _revitElementsView.Refresh();
                    OnPropertyChanged(nameof(RevitElementsFilter));
                }
            }
        }

        private void CreateDispatcherTimer() {
            _timerByChangedMask = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _timerByChangedMask.Tick += (s, e) => {
                _timerByChangedMask.Stop();
                UpdateParamValues();
            };
        }

        private void LoadView() {
            RevitElements = _revitRepository.GetRevitElements();
            List<ElementId> categoryIds = _revitRepository.GetCategoryIds(RevitElements);
            CommonParams = _revitRepository.GetParams(categoryIds);

            LoadConfig();
            ParamUpdateCommand.Execute(null);

            SetElementsFilters();
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
            if(string.IsNullOrEmpty(ParamValueMask)
                || ParamValueMask == _localizationService.GetLocalizedString("MainWindow.EnterParamValueMask")) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ParamValueMaskEmpty");
                return false;
            }

            if(SelectedCommonParam is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.SelectParamToRecord");
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            ParamValueMask = setting?.TaskForWrite ?? _localizationService.GetLocalizedString("MainWindow.EnterParamValueMask");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.TaskForWrite = ParamValueMask;
            _pluginConfig.SaveProjectConfig();
        }

        private void ParamUpdate() {
            CreateDispatcherTimer();
            _timerByChangedMask.Start();
        }

        private void UpdateParamValues() {
            if(ParamValueMask == _localizationService.GetLocalizedString("MainWindow.EnterParamValueMask")) {
                return;
            }
            foreach(RevitElement revitElement in RevitElements) {
                revitElement.SetParamValue(ParamValueMask);
            }
        }

        private void AddParamInMask() {
            if(SelectedCommonParamForAdd != null) {
                ParamValueMask = ParamValueMask.Insert(ParamValueMaskCaretIndex, $"{{{SelectedCommonParamForAdd.ParamName}}}");
                SelectedCommonParamForAdd = null;
            }
        }


        /// <summary>
        /// Назначает фильтр привязанный к тексту, через который фильтруется список категорий в GUI
        /// </summary>
        /// <param name="p"></param>
        private void SetElementsFilters() {
            // Организуем фильтрацию списка категорий
            _revitElementsView = CollectionViewSource.GetDefaultView(RevitElements);
            _revitElementsView.Filter = item => String.IsNullOrEmpty(RevitElementsFilter) ? true :
                ((RevitElement) item).ElemName.IndexOf(RevitElementsFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
