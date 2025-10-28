using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitArchitecturalDocumentation.Models;
internal class ViewHelper {
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Конструктор, применяемый при создании новых видов
    /// </summary>
    public ViewHelper(RevitRepository revitRepository, ILocalizationService localizationService,
                      TreeReportNode report = null) {
        Repository = revitRepository;
        _localizationService = localizationService;
        Report = report;
    }

    /// <summary>
    /// Конструктор, применяемый при анализе существующих видов
    /// </summary>
    public ViewHelper(ViewPlan viewPlan, ILocalizationService localizationService) {
        View = viewPlan;
        NameHelper = new ViewNameHelper(viewPlan);
        _localizationService = localizationService;
    }

    public RevitRepository Repository { get; set; }

    public ViewPlan View { get; set; }
    public TreeReportNode Report { get; set; }
    public ViewNameHelper NameHelper { get; set; }


    /// <summary>
    /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
    /// </summary>
    public ViewPlan GetView(string newViewName, Element visibilityScope = null, ViewFamilyType viewFamilyType = null, Level level = null, ViewPlan viewForDuplicate = null) {

        if(newViewName.Length == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidNameForTask"));
            return null;
        }

        ViewPlan newViewPlan = Repository.FindViewByName(newViewName);
        // Если newViewPlan is null, значит вид с указанным именем не найден в проекте и его нужно создать
        if(newViewPlan is null) {
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewWithName")} " +
                $"\"{newViewName}\" {_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.NotFindLetsCreate")}");

            if(level is null && viewForDuplicate is null) {
                Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidLevelAndVisibilityScope"));
                return null;
            }

            // В зависимости от того, что передали дублируем вид или создаем с нуля
            if(viewForDuplicate != null) {
                DuplicateView(newViewName, viewForDuplicate);
                SetUpView(visibilityScope);
            } else {
                CreateView(newViewName, viewFamilyType, level);
                SetUpView(visibilityScope);
            }

        } else {
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewWithName")} " +
                $"\"{newViewName}\" {_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.SuccessFoundInProject")}");
            View = newViewPlan;
        }

        return newViewPlan;
    }



    /// <summary>
    /// Создает вид на указанном уровне, назначает тип вида и задает указанное имя
    /// </summary>
    public ViewPlan CreateView(string newViewName, ViewFamilyType viewFamilyType, Level level) {

        if(newViewName.Length == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidNameForTask"));
            return null;
        }
        if(viewFamilyType is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidViewType"));
            return null;
        }
        if(level is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidLevelForView"));
            return null;
        }

        ViewPlan newViewPlan = null;
        try {
            newViewPlan = ViewPlan.Create(Repository.Document, viewFamilyType.Id, level.Id);
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewCreatedSuccessfully"));
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.SetViewType")} {viewFamilyType.Name}!");
            newViewPlan.Name = newViewName;
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.NameGiven")} {newViewPlan.Name}");

        } catch(Exception) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.FailedSheetCreation"));
        }
        View = newViewPlan;
        return newViewPlan;
    }


    /// <summary>
    /// Дублирует указанный вид и задает указанное имя
    /// </summary>
    public ViewPlan DuplicateView(string newViewName, ViewPlan viewForDuplicate) {

        if(newViewName.Length == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidNameForTask"));
            return null;
        }

        if(viewForDuplicate is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidViewForDuplicate"));
            return null;
        }

        ViewPlan newViewPlan = null;
        try {
            ElementId newViewPlanId = viewForDuplicate.Duplicate(ViewDuplicateOption.WithDetailing);
            newViewPlan = viewForDuplicate.Document.GetElement(newViewPlanId) as ViewPlan;
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewWasDuplicated"));
            newViewPlan.Name = newViewName;
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.NameGiven")} {newViewName}");

        } catch(Exception) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ErrorDuplicatingView"));
        }
        View = newViewPlan;
        return newViewPlan;
    }


    /// <summary>
    /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
    /// </summary>
    public void SetUpView(Element visibilityScope) {

        if(visibilityScope is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidVisibilityScope"));
            return;
        }

        if(View is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidViewForWork"));
            return;
        }

        try {
            View.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(visibilityScope.Id);
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.SetVisibilityScope")} {visibilityScope.Name}");

            View.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(1);
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.SetAnnotationCropping"));

            var cropManager = View.GetCropRegionShapeManager();
            double dim = UnitUtilsHelper.ConvertToInternalValue(3);
            cropManager.TopAnnotationCropOffset = dim;
            cropManager.BottomAnnotationCropOffset = dim;
            cropManager.LeftAnnotationCropOffset = dim;
            cropManager.RightAnnotationCropOffset = dim;
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.SetMinAnnotationCroppingOffset"));

        } catch(Exception) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidViewSetting"));
        }
    }



    /// <summary>
    /// Размещает вид на листе
    /// </summary>
    public Viewport PlaceViewportOnSheet(ViewSheet viewSheet, ElementType viewportType) {
        // Если переданный лист или вид is null или вид.экран вида нельзя добавить на лист, то возвращаем null
        if(viewSheet is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidSheetForPlace"));
            return null;
        }
        if(View is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.InvalidViewForPlace"));
            return null;
        }
        if(!Viewport.CanAddViewToSheet(Repository.Document, viewSheet.Id, View.Id)) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.CannotPlaceViewOnSheet"));
            return null;
        }

        // Размещаем план на листе в начальной точке, чтобы оценить габариты
        var viewPort = Viewport.Create(Repository.Document, viewSheet.Id, View.Id, new XYZ(0, 0, 0));
        if(viewPort is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.FailedViewportCreation"));
            return null;
        }
        Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewCreatedSuccessfullyOnSheet"));

        if(viewportType != null) {
            viewPort.ChangeTypeId(viewportType.Id);
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.SettedViewType")}" +
                $"{viewportType.Name}!");
        }

        var viewportCenter = viewPort.GetBoxCenter();
        var viewportOutline = viewPort.GetBoxOutline();
        double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
        double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

        // Ищем рамку листа

        if(new FilteredElementCollector(Repository.Document, viewSheet.Id)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .WhereElementIsNotElementType()
            .FirstOrDefault() is not FamilyInstance titleBlock) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.CannotFoundTitleBlockOnSheet"));
            return null;
        }

        Repository.Document.Regenerate();

        // Получение габаритов рамки листа
        var boundingBoxXYZ = titleBlock.get_BoundingBox(viewSheet);
        double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
        double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

        double titleBlockMinY = boundingBoxXYZ.Min.Y;
        double titleBlockMinX = boundingBoxXYZ.Min.X;

        var correctPosition = new XYZ(
            titleBlockMinX + viewportHalfWidth,
            titleBlockMinY + titleBlockHeight / 2,
            0);

        viewPort.SetBoxCenter(correctPosition);
        Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewPositionedOnSheet"));

#if REVIT_2022_OR_GREATER
        viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
        Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.View.ViewTitlePositionedOnSheet"));
#endif
        return viewPort;
    }
}
