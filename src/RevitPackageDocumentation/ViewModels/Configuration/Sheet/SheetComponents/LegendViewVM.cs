using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

/// <summary>
/// Модуль легенды: размещает выбранную легенду на листе
/// </summary>
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
    }

    [Required(ErrorMessage = "Validation.LegendViewIsNull")]
    public View LegendView {
        get => _legendView;
        set => RaiseAndSetIfChanged(ref _legendView, value);
    }

    public FiltrationComboBoxFilterListVM LegendViewFilter {
        get => _legendViewFilter;
        set => RaiseAndSetIfChanged(ref _legendViewFilter, value);
    }

    [Required(ErrorMessage = "Validation.ViewportTypeIsNull")]
    public ElementType ViewportType {
        get => _viewportType;
        set => RaiseAndSetIfChanged(ref _viewportType, value);
    }

    public FiltrationComboBoxFilterListVM ViewportTypeFilter {
        get => _viewportTypeFilter;
        set => RaiseAndSetIfChanged(ref _viewportTypeFilter, value);
    }

    public override void Process(bool processDependent = false) {
        var viewPort = Place();
        SetCustomParams(viewPort);
    }

    /// <summary>
    /// Размещает легенду слева от рамки листа
    /// </summary>
    /// <returns>Созданный видовой экран или null, если листа нет или легенду разместить нельзя</returns>
    public Viewport Place() {
        // Проверяем можем ли разместить на листе легенду
        if(Sheet.SheetInstance is null
            || !Viewport.CanAddViewToSheet(Repository.Document, Sheet.SheetInstance.Id, LegendView.Id)) {
            return null;
        }

        // Размещаем легенду на листе (точка вставки в мм в координатах листа)
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
