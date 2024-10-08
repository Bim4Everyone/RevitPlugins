using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private List<RevitParameter> _commonParamsForRead;
        private List<RevitParameter> _commonParamsForWrite;
        private List<RevitElementViewModel> _revitElementVMs;

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

        public List<RevitElementViewModel> RevitElementVMs {
            get => _revitElementVMs;
            set => this.RaiseAndSetIfChanged(ref _revitElementVMs, value);
        }

        public List<RevitParameter> CommonParamsForRead {
            get => _commonParamsForRead;
            set => this.RaiseAndSetIfChanged(ref _commonParamsForRead, value);
        }

        public List<RevitParameter> CommonParamsForWrite {
            get => _commonParamsForWrite;
            set => this.RaiseAndSetIfChanged(ref _commonParamsForWrite, value);
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
            _timerByChangedMask.Tick += (s, e) => UpdateParamValuesByTimer();
        }

        private void UpdateParamValuesByTimer() {
            _timerByChangedMask.Stop();
            UpdateParamValues();
        }

        private void LoadView() {
            var revitElements = _revitRepository.GetRevitElements();
            RevitElementVMs = revitElements
                .Select(e => new RevitElementViewModel(e))
                .ToList();

            List<ElementId> categoryIds = _revitRepository.GetCategoryIds(revitElements);
            CommonParamsForRead = _revitRepository.GetParamsForRead(categoryIds);
            CommonParamsForWrite = _revitRepository.GetParamsForWrite(CommonParamsForRead);

            LoadConfig();
            ParamUpdateCommand.Execute(null);

            SetElementsFilters();
        }

        private void AcceptView() {
            using(Transaction transaction = _revitRepository.Document.StartTransaction("Изменение значений параметров")) {
                foreach(RevitElementViewModel revitElement in RevitElementVMs) {
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
            ParamValueMask = setting?.ParamValueMask ?? _localizationService.GetLocalizedString("MainWindow.EnterParamValueMask");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.ParamValueMask = ParamValueMask;
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
            foreach(RevitElementViewModel revitElement in RevitElementVMs) {
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
        private void SetElementsFilters() {
            // Организуем фильтрацию списка категорий
            _revitElementsView = CollectionViewSource.GetDefaultView(RevitElementVMs);
            _revitElementsView.Filter = item => String.IsNullOrEmpty(RevitElementsFilter) ? true :
                ((RevitElementViewModel) item).ElemName.IndexOf(RevitElementsFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
