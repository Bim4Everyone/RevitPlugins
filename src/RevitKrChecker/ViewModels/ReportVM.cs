using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private string _selectedErrorTooltips;

        private ObservableCollection<ReportItemVM> _reportResult = new ObservableCollection<ReportItemVM>();

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

            GroupingVM = new ReportGroupingVM();

            LoadViewCommand = RelayCommand.Create(LoadView);
            ClosingViewCommand = RelayCommand.Create(ClosingView);

            ReсheckCommand = RelayCommand.Create(Reсheck);
            UpdateTooltipsCommand = RelayCommand.Create<IEnumerable>(UpdateTooltips);
            ShowErrorElemsCommand = RelayCommand.Create<IEnumerable>(ShowErrorElems);

            Reсheck();
        }

        public ReportGroupingVM GroupingVM { get; }

        public ICommand LoadViewCommand { get; }
        public ICommand ClosingViewCommand { get; }

        public ICommand ReсheckCommand { get; }
        public ICommand UpdateTooltipsCommand { get; }
        public ICommand ShowErrorElemsCommand { get; }

        public ObservableCollection<ReportItemVM> ReportResult {
            get => _reportResult;
            set => this.RaiseAndSetIfChanged(ref _reportResult, value);
        }

        public string SelectedErrorTooltips {
            get => _selectedErrorTooltips;
            set => this.RaiseAndSetIfChanged(ref _selectedErrorTooltips, value);
        }

        private void LoadView() {
            GroupingVM.SetCollection(ReportResult);
            LoadConfig();
        }

        private void ClosingView() {
            SaveConfig();
        }

        private void LoadConfig() {
            var settings = _pluginConfig.GetSettings(_revitRepository.Document);
            if(settings is null) { return; }
            GroupingVM.SelectedThirdLevelGrouping = settings.SelectedThirdLevelGrouping;
            GroupingVM.ThirdLevelGroupingChangedCommand.Execute(null);
            GroupingVM.SelectedSecondLevelGrouping = settings.SelectedSecondLevelGrouping;
            GroupingVM.SecondLevelGroupingChangedCommand.Execute(null);
            GroupingVM.SelectedFirstLevelGrouping = settings.SelectedFirstLevelGrouping;
            GroupingVM.FirstLevelGroupingChangedCommand.Execute(null);
        }

        private void SaveConfig() {
            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.SelectedFirstLevelGrouping = GroupingVM.SelectedFirstLevelGrouping;
            settings.SelectedSecondLevelGrouping = GroupingVM.SelectedSecondLevelGrouping;
            settings.SelectedThirdLevelGrouping = GroupingVM.SelectedThirdLevelGrouping;
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
    }
}
