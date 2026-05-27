using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class StructuralPlanViewVM : SheetComponentVM {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewPlan _viewTemplate;
    private string _viewCount;
    private ViewPlan _viewInstance;

    // Смещение по горизонтали в дюймах слева, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameLeftOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Смещение по вертикали в дюймах сверху, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    public StructuralPlanViewVM(SheetVM sheetVM, RevitRepository revitRepository, ILocalizationService localizationService)
        : base(sheetVM) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
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

    public ViewPlan ViewInstance {
        get => _viewInstance;
        set => RaiseAndSetIfChanged(ref _viewInstance, value);
    }

    public override void CreateComponent() { ViewName += "1"; }

    public override bool ValidateModule() {
        if(string.IsNullOrEmpty(ViewName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(ViewFamilyType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewFamilyTypeIsNull");
            return false;
        }
        if(ViewportType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewportTypeIsNull");
            return false;
        }
        if(ViewTemplate is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewTemplateIsNull");
            return false;
        }
        if(!double.TryParse(ViewCount, out double viewCountAsDouble) || viewCountAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() {
        ViewInstance = Create();
        Place(ViewInstance);
    }

    public ViewPlan Create() {
        var view = _revitRepository.GetViewByName(ViewName) as ViewPlan;

        if(view is null) {
            try {
                if(Sheet.SheetSet.Params.FirstOrDefault(p => p.ParamName == "Опалубка") is not SelectElemParamVM formworkParam) {
                    return null;
                }

                var selectedElem = formworkParam.SelectedElem;
                var levelId = selectedElem.LevelId;
                if(levelId is null) {
                    return null;
                }
                double elementFromLevelOffset = selectedElem.GetParamValue<double>(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

                view = ViewPlan.Create(_revitRepository.Document, ViewFamilyType.Id, levelId);
                view.Name = ViewName;
                view.ViewTemplateId = ViewTemplate.Id;

                PlanViewRange viewRange = view.GetViewRange();
                viewRange.SetOffset(PlanViewPlane.TopClipPlane,
                    elementFromLevelOffset + UnitUtilsHelper.ConvertToInternalValue(500));
                viewRange.SetOffset(PlanViewPlane.CutPlane,
                    elementFromLevelOffset + UnitUtilsHelper.ConvertToInternalValue(200));
                viewRange.SetOffset(PlanViewPlane.BottomClipPlane,
                    elementFromLevelOffset + UnitUtilsHelper.ConvertToInternalValue(-500));
                viewRange.SetOffset(PlanViewPlane.ViewDepthPlane,
                    elementFromLevelOffset + UnitUtilsHelper.ConvertToInternalValue(-500));
                view.SetViewRange(viewRange);

                // Необходимо для перезагрузки габаритов видов перед их размещением, т.к. при назначении 
                // секущего диапазона, видимых категорий, шаблона вида могут изменяться габариты вида
                _revitRepository.Document.Regenerate();
            } catch(Exception) { }
        }
        return view;
    }

    public void Place(ViewPlan view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance != null
            && view != null
            && Viewport.CanAddViewToSheet(_revitRepository.Document, sheetInstance.Id, view.Id)) {

            // Получение габаритов рамки листа
            if(_revitRepository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
                return;
            }
            var boundingBoxXYZ = titleBlock.get_BoundingBox(sheetInstance);
            double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
            double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

            double titleBlockMinY = boundingBoxXYZ.Min.Y;
            double titleBlockMinX = boundingBoxXYZ.Min.X;
            double titleBlockMaxY = boundingBoxXYZ.Max.Y;

            // Получение габаритов видового экрана
            var viewPort = Viewport.Create(_revitRepository.Document, sheetInstance.Id, view.Id, new XYZ(0, 0, 0));
            viewPort.ChangeTypeId(ViewportType.Id);

            var viewportCenter = viewPort.GetBoxCenter();
            var viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            var correctPosition = new XYZ(
                titleBlockMinX + _titleBlockFrameLeftOffset + viewportHalfWidth,
                titleBlockMaxY - _titleBlockFrameTopOffset - viewportHalfHeight,
                0);

            viewPort.SetBoxCenter(correctPosition);

            string viewportNumberAsStr = viewPort.GetParamValue<string>(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
            if(int.TryParse(viewportNumberAsStr, out int viewportNumberAsInt)) {
                viewPort.SetParamValue(BuiltInParameter.VIEWPORT_DETAIL_NUMBER, (100 + viewportNumberAsInt).ToString());
            }

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
#endif
        }
    }
}
