using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitKrChecker.Models;
using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.ViewModels {
    internal class ReportVM : BaseViewModel {
        private readonly List<Element> _elems;
        private readonly List<ICheck> _stoppingChecks;
        private readonly List<ICheck> _nonStoppingChecks;
        private readonly RevitRepository _revitRepository;
        private readonly PluginConfig _pluginConfig;
        private readonly ILocalizationService _localizationService;

        private string _notSelectedItem;
        private List<string> _groupingList;
        private string _selectedErrorTooltips;
        private CollectionView _reportResultCollectionView;

        private ObservableCollection<ReportItemVM> _reportResult = new ObservableCollection<ReportItemVM>();
        private string _selectedFirstLevelGrouping;
        private string _selectedSecondLevelGrouping;
        private string _selectedThirdLevelGrouping;

        public ReportVM(ReportVMOption option) {
            _elems = option.Elements
                ?? throw new ArgumentNullException(nameof(option.Elements));
            _stoppingChecks = option.StoppingChecks
                ?? throw new ArgumentNullException(nameof(option.StoppingChecks));
            _nonStoppingChecks = option.NonStoppingChecks
                ?? throw new ArgumentNullException(nameof(option.NonStoppingChecks));
            _revitRepository = option.RepositoryOfRevit
                ?? throw new ArgumentNullException(nameof(option.RepositoryOfRevit));
            _pluginConfig = option.ConfigOfPlugin
                ?? throw new ArgumentNullException(nameof(option.ConfigOfPlugin));
            _localizationService = option.LocalizationService
                ?? throw new ArgumentNullException(nameof(option.LocalizationService));

            SelectedFirstLevelGrouping = SelectedSecondLevelGrouping = SelectedThirdLevelGrouping = NotSelectedItem = "<Нет>";
            GroupingList = GetGroupingList();

            LoadViewCommand = RelayCommand.Create(LoadView);
            ClosingViewCommand = RelayCommand.Create(ClosingView);

            ReсheckCommand = RelayCommand.Create(Reсheck);
            UpdateTooltipsCommand = RelayCommand.Create<IEnumerable>(UpdateTooltips);
            ShowErrorElemsCommand = RelayCommand.Create<IEnumerable>(ShowErrorElems);

            FirstLevelGroupingChangedCommand = RelayCommand.Create(FirstLevelGroupingChanged);
            SecondLevelGroupingChangedCommand = RelayCommand.Create(SecondLevelGroupingChanged);
            ThirdLevelGroupingChangedCommand = RelayCommand.Create(ThirdLevelGroupingChanged);

            Reсheck();
        }


        public ICommand LoadViewCommand { get; }
        public ICommand ClosingViewCommand { get; }

        public ICommand ReсheckCommand { get; }
        public ICommand UpdateTooltipsCommand { get; }
        public ICommand ShowErrorElemsCommand { get; }

        public ICommand FirstLevelGroupingChangedCommand { get; }
        public ICommand SecondLevelGroupingChangedCommand { get; }
        public ICommand ThirdLevelGroupingChangedCommand { get; }


        public ObservableCollection<ReportItemVM> ReportResult {
            get => _reportResult;
            set => this.RaiseAndSetIfChanged(ref _reportResult, value);
        }

        public string SelectedErrorTooltips {
            get => _selectedErrorTooltips;
            set => this.RaiseAndSetIfChanged(ref _selectedErrorTooltips, value);
        }


        public string NotSelectedItem {
            get => _notSelectedItem;
            set => this.RaiseAndSetIfChanged(ref _notSelectedItem, value);
        }

        public List<string> GroupingList {
            get => _groupingList;
            set => this.RaiseAndSetIfChanged(ref _groupingList, value);
        }

        public string SelectedFirstLevelGrouping {
            get => _selectedFirstLevelGrouping;
            set => this.RaiseAndSetIfChanged(ref _selectedFirstLevelGrouping, value);
        }

        public string SelectedSecondLevelGrouping {
            get => _selectedSecondLevelGrouping;
            set => this.RaiseAndSetIfChanged(ref _selectedSecondLevelGrouping, value);
        }

        public string SelectedThirdLevelGrouping {
            get => _selectedThirdLevelGrouping;
            set => this.RaiseAndSetIfChanged(ref _selectedThirdLevelGrouping, value);
        }



        /// <summary>
        /// Метод, отрабатывающий при загрузке окна
        /// </summary>
        private void LoadView() {
            LoadConfig();
        }

        /// <summary>
        /// Метод, отрабатывающий при закрытии окна
        /// </summary>
        private void ClosingView() {
            SaveConfig();
        }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        private void LoadConfig() {
            var settings = _pluginConfig.GetSettings(_revitRepository.Document);
            if(settings is null) { return; }
            SelectedThirdLevelGrouping = settings.SelectedThirdLevelGrouping;
            ThirdLevelGroupingChangedCommand.Execute(null);
            SelectedSecondLevelGrouping = settings.SelectedSecondLevelGrouping;
            SecondLevelGroupingChangedCommand.Execute(null);
            SelectedFirstLevelGrouping = settings.SelectedFirstLevelGrouping;
            FirstLevelGroupingChangedCommand.Execute(null);
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {
            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.SelectedFirstLevelGrouping = SelectedFirstLevelGrouping;
            settings.SelectedSecondLevelGrouping = SelectedSecondLevelGrouping;
            settings.SelectedThirdLevelGrouping = SelectedThirdLevelGrouping;
            _pluginConfig.SaveProjectConfig();
        }

        private void UpdateTooltips(object obj) {
            var selectedReportItemVMs = obj as IEnumerable;
            string temp = string.Empty;

            foreach(var item in selectedReportItemVMs) {
                ReportItemVM reportItemVM = item as ReportItemVM;
                temp += reportItemVM.ElementErrorTooltip + Environment.NewLine;
            }
            SelectedErrorTooltips = temp;
        }

        private void ShowErrorElems(object obj) {
            var selectedReportItemVMs = obj as IEnumerable;
            List<ElementId> elemIds = new List<ElementId>();

            foreach(var item in selectedReportItemVMs) {
                ReportItemVM reportItemVM = item as ReportItemVM;
                elemIds.Add(reportItemVM.Elem.Id);
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(elemIds);
        }


        private void Reсheck() {
            ReportResult.Clear();
            List<Element> tempElems = _elems.GetRange(0, _elems.Count);

            /* Существует два типа проверок:
            1. Проверки, которые не допускают элемент к другим проверкам в случае ошибки
            _stoppingChecks = List<ICheck>();
            2. Проверки, которые допускают элемент к другим проверкам в случае ошибки
            _nonStoppingChecks = List<ICheck>();*/

            for(int i = tempElems.Count - 1; i >= 0; i--) {
                foreach(ICheck check in _stoppingChecks) {
                    if(!check.Check(tempElems[i], out CheckInfo info)) {
                        ReportResult.Add(new ReportItemVM(info));
                        tempElems.Remove(tempElems[i]);
                        break;
                    }
                }
            }

            foreach(Element element in tempElems) {
                foreach(ICheck check in _nonStoppingChecks) {
                    if(!check.Check(element, out CheckInfo info)) {
                        ReportResult.Add(new ReportItemVM(info));
                    }
                }
            }
        }

        /// <summary>
        /// Метод по получению списка параметров по которым можно будет группировать отчет об ошибках
        /// </summary>
        private List<string> GetGroupingList() {
            List<string> groupingList = new List<string>() { NotSelectedItem };
            foreach(var prop in typeof(ReportItemVM).GetProperties()) {
                groupingList.Add(prop.Name);
            }
            return groupingList;
        }

        /// <summary>
        /// Метод команды по изменению значения группировки списка ошибок на первом уровне
        /// </summary>
        private void FirstLevelGroupingChanged() {
            if(SelectedFirstLevelGrouping is null) {
                return;
            }
            if(SelectedFirstLevelGrouping == NotSelectedItem) {
                SelectedSecondLevelGrouping = SelectedThirdLevelGrouping = NotSelectedItem;
            }
            ReportResultGroupingUpdate();
        }

        /// <summary>
        /// Метод команды по изменению значения группировки списка ошибок на втором уровне
        /// </summary>
        private void SecondLevelGroupingChanged() {
            if(SelectedSecondLevelGrouping is null) {
                return;
            }
            if(SelectedSecondLevelGrouping == NotSelectedItem) {
                SelectedThirdLevelGrouping = NotSelectedItem;
            }
            ReportResultGroupingUpdate();
        }

        /// <summary>
        /// Метод команды по изменению значения группировки списка ошибок на третьем уровне
        /// </summary>
        private void ThirdLevelGroupingChanged() {
            if(SelectedThirdLevelGrouping is null) {
                return;
            }
            ReportResultGroupingUpdate();
        }

        /// <summary>
        /// Метод по обновлению группировки списка ошибок в соответствии с выбранной группировкой на каждом уровне
        /// </summary>
        private void ReportResultGroupingUpdate() {
            if(_reportResultCollectionView is null) {
                _reportResultCollectionView = (CollectionView) CollectionViewSource.GetDefaultView(ReportResult);
            }
            if(_reportResultCollectionView is null) {
                return;
            }

            _reportResultCollectionView.GroupDescriptions.Clear();

            if(!string.IsNullOrEmpty(SelectedFirstLevelGrouping) && SelectedFirstLevelGrouping != NotSelectedItem) {
                PropertyGroupDescription group1 = new PropertyGroupDescription(SelectedFirstLevelGrouping);
                _reportResultCollectionView.GroupDescriptions.Add(group1);
            }

            if(!string.IsNullOrEmpty(SelectedSecondLevelGrouping) && SelectedSecondLevelGrouping != NotSelectedItem) {
                PropertyGroupDescription group2 = new PropertyGroupDescription(SelectedSecondLevelGrouping);
                _reportResultCollectionView.GroupDescriptions.Add(group2);
            }

            if(!string.IsNullOrEmpty(SelectedThirdLevelGrouping) && SelectedThirdLevelGrouping != NotSelectedItem) {
                PropertyGroupDescription group3 = new PropertyGroupDescription(SelectedThirdLevelGrouping);
                _reportResultCollectionView.GroupDescriptions.Add(group3);
            }
        }
    }
}
