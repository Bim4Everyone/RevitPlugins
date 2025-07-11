using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitKrChecker.Models;
using RevitKrChecker.Views;

namespace RevitKrChecker.ViewModels;
internal class MainVM : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ReportVM _reportVM;
    private readonly IResolutionRoot _resolutionRoot;

    public MainVM(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ReportVM reportVM,
        IResolutionRoot resolutionRoot) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _reportVM = reportVM;
        _resolutionRoot = resolutionRoot;

        CheckElemsFromViewCommand = RelayCommand.Create(CheckElemsFromView);
        CheckElemsFromPjCommand = RelayCommand.Create(CheckElemsFromPj);
    }


    public ICommand CheckElemsFromViewCommand { get; }
    public ICommand CheckElemsFromPjCommand { get; }

    private void CheckElemsFromView() {
        var elements = _revitRepository.GetViewElements();
        ShowReport(elements);
    }

    private void CheckElemsFromPj() {
        var elements = _revitRepository.GetPjElements();
        ShowReport(elements);
    }

    private void ShowReport(List<Element> elements) {
        _reportVM.CheckElements(elements);

        var reportWindow = _resolutionRoot.Get<ReportWindow>(
            new PropertyValue(
                nameof(ReportWindow.DataContext),
                _reportVM));
        reportWindow.Show();
    }
}
