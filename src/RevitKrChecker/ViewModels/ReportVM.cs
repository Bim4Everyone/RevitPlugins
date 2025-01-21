using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

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
        private string _selectedErrorTooltips;

        private ObservableCollection<ReportItemVM> _reportResult = new ObservableCollection<ReportItemVM>();

        public ReportVM(List<Element> elements, List<ICheck> stoppingChecks, List<ICheck> nonStoppingChecks, RevitRepository revitRepository) {
            _elems = elements;
            _stoppingChecks = stoppingChecks;
            _nonStoppingChecks = nonStoppingChecks;
            _revitRepository = revitRepository;

            ReсheckCommand = RelayCommand.Create(Reсheck);
            UpdateTooltipsCommand = RelayCommand.Create<IEnumerable>(UpdateTooltips);
            ShowErrorElemsCommand = RelayCommand.Create<IEnumerable>(ShowErrorElems);

            Reсheck();
        }

        public ICommand ReсheckCommand { get; }
        public ICommand UpdateTooltipsCommand { get; }
        public ICommand ShowErrorElemsCommand { get; }


        public string SelectedErrorTooltips {
            get => _selectedErrorTooltips;
            set => this.RaiseAndSetIfChanged(ref _selectedErrorTooltips, value);
        }

        public ObservableCollection<ReportItemVM> ReportResult {
            get => _reportResult;
            set => this.RaiseAndSetIfChanged(ref _reportResult, value);
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
    }
}
