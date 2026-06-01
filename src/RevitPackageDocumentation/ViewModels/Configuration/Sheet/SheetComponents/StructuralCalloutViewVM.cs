using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class StructuralCalloutViewVM : SheetComponentVM {
    // Ширина фрагмента
    private readonly double _calloutWidth = UnitUtilsHelper.ConvertToInternalValue(4000);

    // Высота фрагмента
    private readonly double _calloutHeight = UnitUtilsHelper.ConvertToInternalValue(3000);

    // Отступ между фрагментами в модели
    private readonly double _viewsOffset = UnitUtilsHelper.ConvertToInternalValue(5000);

    // Отступ между видовыми экранами на листе
    private readonly double _viewportOffset = UnitUtilsHelper.ConvertToInternalValue(100);

    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewPlan _viewTemplate;
    private string _viewCount;
    private string _viewportNumber;

    public StructuralCalloutViewVM(
        SheetVM sheetVM,
        RevitRepository repository,
        ILocalizationService localizationService,
        StringParamSetService stringParamSetService)
        : base(sheetVM, repository, localizationService, stringParamSetService) {
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
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

    public ViewPlan ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public string ViewportNumber {
        get => _viewportNumber;
        set => RaiseAndSetIfChanged(ref _viewportNumber, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(string.IsNullOrEmpty(ViewName)) {
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
        if(!double.TryParse(ViewCount, out double viewCountAsDouble) || viewCountAsDouble < 1) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
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

    public View Create(int number) {
        var viewName = $"{ViewName}_{number}";
        var view = Repository.GetViewByName(viewName);

        if(view != null) {
            return view;
        }

        try {
            var index = Sheet.SheetComponents.IndexOf(this);
            StructuralPlanViewVM parentView = default;

            for(int i = index; i >= 0; i--) {
                parentView = Sheet.SheetComponents[i] as StructuralPlanViewVM;

                if(parentView != null) {
                    break;
                }
            }

            if(parentView is null || parentView.ViewInstance is null) {
                return null;
            }

            // Рассчитываем точки размещения фрагмента относительно центра опалубки 
            if(Sheet.SheetSet.Params.FirstOrDefault(p => p.ParamName == "Опалубка") is not SelectElemParamVM formworkParam) {
                return null;
            }
            var selectedElem = formworkParam.SelectedElem;
            var bbox = selectedElem.get_BoundingBox(null);

            var calloutStart = (bbox.Min + bbox.Max) / 2 + new XYZ(_viewsOffset * (number - 1), -_viewsOffset, 0);
            var calloutEnd = calloutStart + new XYZ(_calloutWidth, _calloutHeight, 0);

            view = ViewSection.CreateCallout(
                Repository.Document,
                parentView.ViewInstance.Id,
                ViewFamilyType.Id,
                calloutStart,
                calloutEnd);
            view.Name = viewName;
            view.ViewTemplateId = ViewTemplate.Id;

            // Необходимо для перезагрузки габаритов видов перед их размещением, т.к. при назначении 
            // секущего диапазона, видимых категорий, шаблона вида могут изменяться габариты вида
            Repository.Document.Regenerate();
        } catch(System.Exception) { }
        return view;
    }

    public void Place(View view, int number) {
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
                titleBlockMinY - viewportHalfHeight - _viewportOffset,
                0);

            viewPort.SetBoxCenter(correctPosition);

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
#endif
        }
    }
}
