using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitKrChecker.Models;
using RevitKrChecker.Models.Check;

namespace RevitKrChecker.ViewModels {
    internal class ReportVM : BaseViewModel {
        private List<ReportItemVM> _reportResult;

        public ReportVM(List<Element> elements, List<ICheck> stoppingChecks, List<ICheck> nonStoppingChecks) {
            Elems = elements;
            StoppingChecks = stoppingChecks;
            NonStoppingChecks = nonStoppingChecks;

            ReсheckCommand = RelayCommand.Create(Reсheck);
            Reсheck();
        }

        public ICommand ReсheckCommand { get; }

        public List<Element> Elems { get; set; }
        public List<ICheck> StoppingChecks { get; set; }
        public List<ICheck> NonStoppingChecks { get; set; }


        public List<ReportItemVM> ReportResult {
            get => _reportResult;
            set => this.RaiseAndSetIfChanged(ref _reportResult, value);
        }

        private void Reсheck() {
            ReportResult = new List<ReportItemVM>();

            /* Существует два типа проверок:
            1. Проверки, которые не допускают элемент к другим проверкам в случае ошибки
            _stoppingChecks = List<ICheck>();
            2. Проверки, которые допускают элемент к другим проверкам в случае ошибки
            _nonStoppingChecks = List<ICheck>();*/

            for(int i = Elems.Count - 1; i >= 0; i--) {
                foreach(ICheck check in StoppingChecks) {
                    if(!check.Check(Elems[i], out CheckInfo info)) {
                        ReportResult.Add(new ReportItemVM(info));
                        Elems.Remove(Elems[i]);
                        break;
                    }
                }
            }

            foreach(Element element in Elems) {
                foreach(ICheck check in NonStoppingChecks) {
                    if(!check.Check(element, out CheckInfo info)) {
                        ReportResult.Add(new ReportItemVM(info));
                    }
                }
            }
        }
    }
}
