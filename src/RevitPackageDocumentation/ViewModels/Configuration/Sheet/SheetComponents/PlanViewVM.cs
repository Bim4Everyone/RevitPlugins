using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class PlanViewVM : SheetComponentVM {
    // При разработке документации пользователям необходимо иметь возможность задать одинаковый Номер вида
    // для нескольких компонентов одного листа (значение задается одинаковое, но за счет разного типа вида в заголовке
    // видового экрана на листе отображается по разному).
    // По умолчанию в Revit это не допускается, поэтому пользователи используют регламентированные стандартами невидимые
    // символы для различных типов видов. Для отличия Номера вида у планов регламентирован невидимый символ
    // указанный ниже (в Юникоде это Left-to-Right Mark (LRM))
    private readonly string _uniqueViewportNumberKey = "‎";

    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewPlan _viewTemplate;
    private string _viewCount;
    private SelectElemParamVM _selectedSelectElemParam;
    private ViewPlan _viewInstance;

    private FiltrationComboBoxFilterListVM _viewportTypeFilter;
    private FiltrationComboBoxFilterListVM _viewFamilyTypeFilter;
    private FiltrationComboBoxFilterListVM _viewTemplateFilter;
    private bool _viewTemplateRemoveAfterCreation = false;

    // Смещение по горизонтали в дюймах слева, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameLeftOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Смещение по вертикали в дюймах сверху, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    public PlanViewVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
    }

    [Required(ErrorMessage = "Validation.ViewNameIsEmpty")]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\];~]+$", ErrorMessage = "Validation.ViewNameIsNotCorrect")]
    public string ViewNameFormula {
        get => _viewNameFormula;
        set => RaiseAndSetIfChanged(ref _viewNameFormula, value);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    [Required(ErrorMessage = "Validation.ViewFamilyTypeIsNull")]
    public ViewFamilyType ViewFamilyType {
        get => _viewFamilyType;
        set => RaiseAndSetIfChanged(ref _viewFamilyType, value);
    }

    public FiltrationComboBoxFilterListVM ViewFamilyTypeFilter {
        get => _viewFamilyTypeFilter;
        set => RaiseAndSetIfChanged(ref _viewFamilyTypeFilter, value);
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

    [Required(ErrorMessage = "Validation.ViewTemplateIsNull")]
    public ViewPlan ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public FiltrationComboBoxFilterListVM ViewTemplateFilter {
        get => _viewTemplateFilter;
        set => RaiseAndSetIfChanged(ref _viewTemplateFilter, value);
    }

    public bool ViewTemplateRemoveAfterCreation {
        get => _viewTemplateRemoveAfterCreation;
        set => RaiseAndSetIfChanged(ref _viewTemplateRemoveAfterCreation, value);
    }

    [PositiveInteger(ErrorMessage = "Validation.ViewCountIsNotCorrect")]
    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    [Required(ErrorMessage = "Validation.SelectedSelectElemParamIsNull")]
    [ChildHasErrors(ErrorMessage = "Validation.SelectElemParamSelectedElemIsNull")]
    public SelectElemParamVM SelectedSelectElemParam {
        get => _selectedSelectElemParam;
        set => RaiseAndSetIfChanged(ref _selectedSelectElemParam, value);
    }

    public ViewPlan ViewInstance {
        get => _viewInstance;
        set => RaiseAndSetIfChanged(ref _viewInstance, value);
    }

    public override void Process(bool processDependent = false) {
        ViewInstance = Create();
        var viewPort = Place(ViewInstance);
        SetCustomParams(viewPort);
    }

    public ViewPlan Create() {
        var view = Repository.GetViewByName(ViewName) as ViewPlan;

        if(view is null) {
            try {
                var selectedElem = SelectedSelectElemParam.SelectedElem;
                var levelId = selectedElem.LevelId;
                if(levelId is null) {
                    return null;
                }
                double elementFromLevelOffset = selectedElem.GetParamValue<double>(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

                view = ViewPlan.Create(Repository.Document, ViewFamilyType.Id, levelId);
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
                Repository.Document.Regenerate();

                // Снимаем шаблон вида, если запросил пользователь
                if(ViewTemplateRemoveAfterCreation) {
                    view.ViewTemplateId = ElementId.InvalidElementId;
                }
            } catch(Exception) { }
        }
        return view;
    }

    public Viewport Place(ViewPlan view) {
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

            double titleBlockMinY = boundingBoxXYZ.Min.Y;
            double titleBlockMinX = boundingBoxXYZ.Min.X;
            double titleBlockMaxY = boundingBoxXYZ.Max.Y;

            var lastViewportInTitleBlock = GetLastViewport<ViewPlan>(vp => vp.GetBoxCenter().Y < titleBlockMaxY);

            // Создание видового экрана
            var viewPort = Viewport.Create(Repository.Document, sheetInstance.Id, view.Id, new XYZ(0, 0, 0));
            viewPort.ChangeTypeId(ViewportType.Id);

            var viewportCenter = viewPort.GetBoxCenter();
            var viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            double correctPositionX = titleBlockMinX + _titleBlockFrameLeftOffset + viewportHalfWidth;
            double correctPositionY = titleBlockMaxY - _titleBlockFrameTopOffset - viewportHalfHeight;
            if(lastViewportInTitleBlock is not null) {
                correctPositionY = titleBlockMaxY + viewportHalfHeight;

                var lastViewportAboveTitleBlock = GetLastViewport<ViewPlan>(vp => vp.GetBoxCenter().Y > titleBlockMaxY);
                if(lastViewportAboveTitleBlock is not null) {
                    correctPositionX = lastViewportAboveTitleBlock.GetBoxOutline().MaximumPoint.X + viewportHalfWidth;
                }
            }
            var correctPosition = new XYZ(
                correctPositionX,
                correctPositionY,
                0);

            string viewPortNumberAsStr =
                _uniqueViewportNumberKey + (GetLastViewportNumber(_uniqueViewportNumberKey, 0, 100) + 1);

            viewPort.SetBoxCenter(correctPosition);
            viewPort.SetParamValue(BuiltInParameter.VIEWPORT_DETAIL_NUMBER, viewPortNumberAsStr);

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
#endif
            return viewPort;
        }
        return null;
    }
}
