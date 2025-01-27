using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitKrChecker.Models;
using RevitKrChecker.Views;

namespace RevitKrChecker.ViewModels {
    internal class MainVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public MainVM(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;


            CheckElemsFromViewCommand = RelayCommand.Create(CheckElemsFromView);
            CheckElemsFromPjCommand = RelayCommand.Create(CheckElemsFromPj);
        }


        public ICommand CheckElemsFromViewCommand { get; }
        public ICommand CheckElemsFromPjCommand { get; }

        private void CheckElemsFromView() {
            List<Element> elements = _revitRepository.GetViewElements();
            GetReportVM(elements);
        }

        private void CheckElemsFromPj() {
            List<Element> elements = _revitRepository.GetPjElements();
            GetReportVM(elements);
        }

        private void GetReportVM(List<Element> elements) {
            ReportVMOption reportVMOption = new ReportVMOption() {
                Elements = elements,
                StoppingChecks = _revitRepository.StoppingChecks(),
                NonStoppingChecks = _revitRepository.NonStoppingChecks(),
                RepositoryOfRevit = _revitRepository,
                ConfigOfPlugin = _pluginConfig,
                LocalizationService = _localizationService
            };
            ReportVM reportVM = new ReportVM(reportVMOption);
            ReportWindow reportWindow = new ReportWindow();
            reportWindow.DataContext = reportVM;
            reportWindow.Show();
        }
    }
}
