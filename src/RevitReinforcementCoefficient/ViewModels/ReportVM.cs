using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class ReportVM : BaseViewModel {
        private readonly IReportService _reportService;
        private readonly RevitRepository _revitRepository;
        private List<ReportItem> _reportItems = new List<ReportItem>();

        public ReportVM(IReportService reportService, RevitRepository revitRepository) {
            _reportService = reportService;
            _revitRepository = revitRepository;

            LoadViewCommand = RelayCommand.Create(LoadView);
            ShowSelectedErrorElementsCommand = RelayCommand.Create(ShowSelectedErrorElements, CanShowSelectedErrorElements);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand ShowSelectedErrorElementsCommand { get; }

        public List<ReportItem> ReportItems {
            get => _reportItems;
            set => this.RaiseAndSetIfChanged(ref _reportItems, value);
        }


        private void LoadView() {
            ReportItems = _reportService.ReportItems;
        }



        //internal void Add(string paramName, ElementId elementId) {
        //    ReportItem error = ReportItems.FirstOrDefault(e => e.ErrorName.Contains($"\"{paramName}\""));

        //    if(error is null) {
        //        ReportItems.Add(new ReportItem(paramName, elementId));
        //    } else {
        //        error.AddIdIfNotContained(elementId);
        //    }
        //}

        private void ShowSelectedErrorElements() {
            List<ElementId> ids = new List<ElementId>();

            foreach(ReportItem reportItem in ReportItems.Where(o => o.IsCheck)) {
                ids.AddRange(reportItem.ElementIds);
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(ids);
        }

        private bool CanShowSelectedErrorElements() {
            return ReportItems.Any(o => o.IsCheck);
        }
    }
}
