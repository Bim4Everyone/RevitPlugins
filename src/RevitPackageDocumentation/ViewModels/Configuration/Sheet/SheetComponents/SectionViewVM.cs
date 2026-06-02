using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class SectionViewVM : SheetComponentVM {
    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewSection _viewTemplate;
    private string _viewCount;
    private SelectElemParamVM _selectedSelectElemParam;

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
        SheetVM sheetVM,
        RevitRepository repository,
        ILocalizationService localizationService,
        StringParamSetService stringParamSetService)
        : base(sheetVM, repository, localizationService, stringParamSetService) {
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
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

    public ElementType ViewportType {
        get => _viewportType;
        set => RaiseAndSetIfChanged(ref _viewportType, value);
    }

    public ViewSection ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public SelectElemParamVM SelectedSelectElemParam {
        get => _selectedSelectElemParam;
        set => RaiseAndSetIfChanged(ref _selectedSelectElemParam, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
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

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() {
        int.TryParse(ViewCount, out int viewCountAsInt);

        for(int i = 1; i <= viewCountAsInt; i++) {
            var view = Create(i);
            Place(view, i);
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

    public void Place(ViewSection view, int number) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance != null
            && view != null
            && Viewport.CanAddViewToSheet(Repository.Document, sheetInstance.Id, view.Id)) {

            // Получение габаритов рамки листа
            if(Repository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
                return;
            }
            var boundingBoxXYZ = titleBlock.get_BoundingBox(sheetInstance);
            double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
            double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

            double titleBlockMinX = boundingBoxXYZ.Min.X;
            double titleBlockMinY = boundingBoxXYZ.Min.Y;

            // Получение габаритов видового экрана
            var viewPort = Viewport.Create(Repository.Document, sheetInstance.Id, view.Id, XYZ.Zero);
            viewPort.ChangeTypeId(ViewportType.Id);

            var viewportCenter = viewPort.GetBoxCenter();
            var viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            var correctPosition = new XYZ(
                titleBlockMinX + viewportHalfWidth + _viewportOffset * (number - 1),
                titleBlockMinY - viewportHalfHeight,
                0);

            viewPort.SetBoxCenter(correctPosition);

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
#endif
        }
    }
}
