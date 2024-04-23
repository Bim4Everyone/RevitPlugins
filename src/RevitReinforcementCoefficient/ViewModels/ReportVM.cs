using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class ReportVM : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public ReportVM(RevitRepository revitRepository) {

            _revitRepository = revitRepository;

            ShowSelectedErrorElementsCommand = RelayCommand.Create(ShowSelectedErrorElements, CanShowSelectedErrorElements);
        }
        public ICommand ShowSelectedErrorElementsCommand { get; }

        public List<ReportItem> ReportItems { get; set; } = new List<ReportItem>();

        internal void Add(string paramName, ElementId elementId) {

            ReportItem error = ReportItems.FirstOrDefault(e => e.ErrorName.Contains($"\"{paramName}\""));

            if(error is null) {

                ReportItems.Add(new ReportItem(paramName, elementId));
            } else {

                error.AddIdIfNotContained(elementId);
            }
        }


        private void ShowSelectedErrorElements() {

            List<ElementId> ids = new List<ElementId>();

            foreach(ReportItem reportItem in ReportItems.Where(o => o.IsCheck)) {

                ids.AddRange(reportItem.ElementIds);
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(ids);
        }

        private bool CanShowSelectedErrorElements() {

            return ReportItems.FirstOrDefault(o => o.IsCheck) is null ? false : true;
        }
    }
}
