using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class LegendViewVM : SheetComponentVM {
    private View _legendView;
    private ElementType _viewportType;

    private FiltrationComboBoxFilterListVM _legendViewFilter;
    private FiltrationComboBoxFilterListVM _viewportTypeFilter;

    public LegendViewVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        CreateComponentCommand = RelayCommand.Create(CreateComponent, Validate);
    }

    public View LegendView {
        get => _legendView;
        set => RaiseAndSetIfChanged(ref _legendView, value);
    }

    public FiltrationComboBoxFilterListVM LegendViewFilter {
        get => _legendViewFilter;
        set => RaiseAndSetIfChanged(ref _legendViewFilter, value);
    }

    public ElementType ViewportType {
        get => _viewportType;
        set => RaiseAndSetIfChanged(ref _viewportType, value);
    }

    public FiltrationComboBoxFilterListVM ViewportTypeFilter {
        get => _viewportTypeFilter;
        set => RaiseAndSetIfChanged(ref _viewportTypeFilter, value);
    }

    public override bool Validate() {
        if(LegendView is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.LegendViewIsNull");
            return false;
        }
        if(ViewportType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewportTypeIsNull");
            return false;
        }
        foreach(var param in CustomParamsList.Params) {
            if(string.IsNullOrEmpty(param.ParamName)) {
                ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.CustomParamsIsNotCorrect");
                return false;
            }
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process(bool processDependent = false) {
        var viewPort = Place();
        SetCustomParams(viewPort);
    }

    public Viewport Place() {
        // Проверяем можем ли разместить на листе легенду
        if(!Viewport.CanAddViewToSheet(Repository.Document, Sheet.SheetInstance.Id, LegendView.Id)) {
            return null;
        }

        // Размещаем легенду на листе
        var position = new XYZ(
            UnitUtilsHelper.ConvertToInternalValue(-100),
            UnitUtilsHelper.ConvertToInternalValue(350),
            0);
        var viewPort = Viewport.Create(Repository.Document,
                                       Sheet.SheetInstance.Id,
                                       LegendView.Id,
                                       position);

        // Задание правильного типа видового экрана
        viewPort.ChangeTypeId(ViewportType.Id);
        return viewPort;
    }
}
