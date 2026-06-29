using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class SectionViewVM : SheetComponentVM {
    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewSection _viewTemplate;
    private string _viewCount;
    private SelectElemParamVM _selectedSelectElemParam;

    private FiltrationComboBoxFilterListVM _viewFamilyTypeFilter;
    private FiltrationComboBoxFilterListVM _viewportTypeFilter;
    private FiltrationComboBoxFilterListVM _viewTemplateFilter;

    // Расстояние до дальней секущей плоскости сечения
    private readonly double _viewDepth = UnitUtilsHelper.ConvertToInternalValue(3000);

    // Ширина подрезки вида
    private readonly double _viewWidth = UnitUtilsHelper.ConvertToInternalValue(3000);

    // Высота подрезки вида
    private readonly double _viewHeight = UnitUtilsHelper.ConvertToInternalValue(1500);

    // Отступ между сечениями в модели
    private readonly double _viewsOffset = UnitUtilsHelper.ConvertToInternalValue(1500);

    // Отступ между видовыми экранами на листе
    private readonly double _viewportOffset = UnitUtilsHelper.ConvertToInternalValue(200);

    public SectionViewVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        CreateComponentCommand = RelayCommand.Create(CreateComponent, Validate);
    }

    public string ViewNameFormula {
        get => _viewNameFormula;
        set => RaiseAndSetIfChanged(ref _viewNameFormula, value);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public ViewFamilyType ViewFamilyType {
        get => _viewFamilyType;
        set => RaiseAndSetIfChanged(ref _viewFamilyType, value);
    }

    public FiltrationComboBoxFilterListVM ViewFamilyTypeFilter {
        get => _viewFamilyTypeFilter;
        set => RaiseAndSetIfChanged(ref _viewFamilyTypeFilter, value);
    }

    public ElementType ViewportType {
        get => _viewportType;
        set => RaiseAndSetIfChanged(ref _viewportType, value);
    }

    public FiltrationComboBoxFilterListVM ViewportTypeFilter {
        get => _viewportTypeFilter;
        set => RaiseAndSetIfChanged(ref _viewportTypeFilter, value);
    }

    public ViewSection ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public FiltrationComboBoxFilterListVM ViewTemplateFilter {
        get => _viewTemplateFilter;
        set => RaiseAndSetIfChanged(ref _viewTemplateFilter, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public SelectElemParamVM SelectedSelectElemParam {
        get => _selectedSelectElemParam;
        set => RaiseAndSetIfChanged(ref _selectedSelectElemParam, value);
    }

    public override bool Validate() {
        if(string.IsNullOrEmpty(ViewNameFormula)) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(ViewFamilyType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewFamilyTypeIsNull");
            return false;
        }
        if(ViewportType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewportTypeIsNull");
            return false;
        }
        if(ViewTemplate is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewTemplateIsNull");
            return false;
        }
        if(!int.TryParse(ViewCount, out int viewCountAsInt) || viewCountAsInt < 1) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
            return false;
        }
        if(SelectedSelectElemParam is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.SelectedSelectElemParamIsNull");
            return false;
        }
        if(SelectedSelectElemParam.SelectedElem is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.SelectedSelectElemParamValueIsNull");
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
        int.TryParse(ViewCount, out int viewCountAsInt);

        for(int i = 1; i <= viewCountAsInt; i++) {
            var view = Create(i);
            var viewPort = Place(view, i);
            SetCustomParams(viewPort);
        }
    }

    public ViewSection Create(int number) {
        var viewName = $"{ViewName}_{number}";
        var view = Repository.GetViewByName(viewName) as ViewSection;

        if(view != null) {
            return view;
        }

        try {
            var selectedElem = SelectedSelectElemParam.SelectedElem;
            var bbox = selectedElem.get_BoundingBox(null);

            // Ориентируем взгляд вдоль оси X (вправо вдоль Y, вверх вдоль Z)
            var t = Transform.Identity;
            t.Origin = (bbox.Min + bbox.Max) / 2 + new XYZ(_viewsOffset * (number - 1), 0, 0);
            t.BasisX = XYZ.BasisY;
            t.BasisY = XYZ.BasisZ;
            t.BasisZ = XYZ.BasisX;

            var sectionBox = new BoundingBoxXYZ {
                Transform = t,
                Min = new XYZ(
                    -_viewWidth / 2,
                    -_viewHeight / 2,
                    0),
                Max = new XYZ(
                    _viewWidth / 2,
                    _viewHeight / 2,
                    _viewDepth)
            };

            view = ViewSection.CreateSection(Repository.Document, ViewFamilyType.Id, sectionBox);
            view.Name = viewName;
            view.ViewTemplateId = ViewTemplate.Id;
            view.SetParamValue(BuiltInParameter.SECTION_COARSER_SCALE_PULLDOWN_METRIC, 100);

            // Необходимо для перезагрузки габаритов видов перед их размещением, т.к. при назначении 
            // секущего диапазона, видимых категорий, шаблона вида могут изменяться габариты вида
            Repository.Document.Regenerate();
        } catch(System.Exception) { }
        return view;
    }

    public Viewport Place(ViewSection view, int number) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance != null
            && view != null
            && Viewport.CanAddViewToSheet(Repository.Document, sheetInstance.Id, view.Id)) {

            // Получение габаритов рамки листа
            if(Repository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
                return null;
            }
            var boundingBoxXYZ = titleBlock.get_BoundingBox(sheetInstance);
            double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
            double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

            double titleBlockMinX = boundingBoxXYZ.Min.X;
            double titleBlockMinY = boundingBoxXYZ.Min.Y;

            int viewPortNumber = GetLastViewportNumber(0, 100) + 1;
            var lastViewport = GetLastViewport<ViewSection>(vp => vp.GetBoxCenter().Y < 0);

            // Создание видового экрана
            var viewPort = Viewport.Create(Repository.Document, sheetInstance.Id, view.Id, XYZ.Zero);
            viewPort.ChangeTypeId(ViewportType.Id);

            var viewportCenter = viewPort.GetBoxCenter();
            var viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            double correctPositionX = lastViewport is null
                ? titleBlockMinX + viewportHalfWidth
                : lastViewport.GetBoxOutline().MaximumPoint.X + viewportHalfWidth;

            var correctPosition = new XYZ(
                correctPositionX,
                titleBlockMinY - viewportHalfHeight,
                0);

            viewPort.SetBoxCenter(correctPosition);
            viewPort.SetParamValue(BuiltInParameter.VIEWPORT_DETAIL_NUMBER, viewPortNumber.ToString());

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
#endif
            return viewPort;
        }
        return null;
    }
}
