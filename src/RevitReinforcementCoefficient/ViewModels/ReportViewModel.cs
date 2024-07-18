using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class ReportViewModel : BaseViewModel {
        private readonly IReportService _reportService;
        private readonly RevitRepository _revitRepository;
        private readonly DesignTypeListVM _designTypeListVM;

        private ObservableCollection<ReportItem> _reportItems = new ObservableCollection<ReportItem>();

        public ReportViewModel(IReportService reportService, RevitRepository revitRepository, DesignTypeListVM designTypeListVM) {
            _reportService = reportService;
            _revitRepository = revitRepository;
            _designTypeListVM = designTypeListVM;

            _designTypeListVM.UpdateReportData += UpdateReportData;
            ShowSelectedErrorElementsCommand = RelayCommand.Create(ShowSelectedErrorElements, CanShowSelectedErrorElements);
        }

        public ICommand ShowSelectedErrorElementsCommand { get; }

        public ObservableCollection<ReportItem> ReportItems {
            get => _reportItems;
            set => this.RaiseAndSetIfChanged(ref _reportItems, value);
        }

        private void UpdateReportData() => ReportItems = new ObservableCollection<ReportItem>(_reportService.ReportItems);

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
