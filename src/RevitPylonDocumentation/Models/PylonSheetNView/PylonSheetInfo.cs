using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
using RevitPylonDocumentation.Models.Services;

using Document = Autodesk.Revit.DB.Document;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonSheetInfo {
    internal CreationSettings Settings { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfoManager Manager { get; set; }
    internal PylonRebarInfo RebarInfo { get; set; }
    internal PylonElemsInfo ElemsInfo { get; set; }
    public ParamValueService ParamValService { get; set; }
    public RebarFinderService RebarFinder { get; set; }

    public bool SheetInProject { get; set; } = false;

    // Марка пилона 
    public string PylonKeyName { get; set; }
    public string ProjectSection { get; set; }

    public List<Element> HostElems { get; set; } = [];
    public ViewSheet PylonViewSheet { get; set; }


    // Рамка листа
    public FamilyInstance TitleBlock { get; set; }
    public double TitleBlockHeight { get; set; }
    public double TitleBlockWidth { get; set; }


    // Видовые экраны разрезов
    public PylonView GeneralView { get; set; }
    public PylonView GeneralViewRebar { get; set; }
    public PylonView GeneralViewPerpendicular { get; set; }
    public PylonView GeneralViewPerpendicularRebar { get; set; }
    public PylonView TransverseViewFirst { get; set; }
    public PylonView TransverseViewSecond { get; set; }
    public PylonView TransverseViewThird { get; set; }
    public PylonView TransverseViewFirstRebar { get; set; }
    public PylonView TransverseViewSecondRebar { get; set; }
    public PylonView TransverseViewThirdRebar { get; set; }


    // Видовые экраны спецификаций
    public PylonView MaterialSchedule { get; set; }
    public PylonView SystemPartsSchedule { get; set; }
    public PylonView IfcPartsSchedule { get; set; }
    public PylonView SkeletonSchedule { get; set; }
    public PylonView SkeletonByElemsSchedule { get; set; }


    // Легенда примечаний
    public PylonView LegendView { get; set; }


    internal void InitializeComponents() {
        Manager = new PylonSheetInfoManager(Settings, Repository, this);

        GeneralView = new PylonView(Settings, Repository, this,
                            AnnotationCreatorFactories.General);
        GeneralViewPerpendicular = new PylonView(Settings, Repository, this,
                                                 AnnotationCreatorFactories.GeneralPerp);
        GeneralViewRebar = new PylonView(Settings, Repository, this,
                                         AnnotationCreatorFactories.GeneralRebar);
        GeneralViewPerpendicularRebar = new PylonView(Settings, Repository, this,
                                                      AnnotationCreatorFactories.GeneralRebarPerp);
        TransverseViewFirst = new PylonView(Settings, Repository, this,
                                            AnnotationCreatorFactories.TransverseFirst);
        TransverseViewSecond = new PylonView(Settings, Repository, this,
                                             AnnotationCreatorFactories.TransverseSecond);
        TransverseViewThird = new PylonView(Settings, Repository, this,
                                            AnnotationCreatorFactories.TransverseThird);
        TransverseViewFirstRebar = new PylonView(Settings, Repository, this,
                                                 AnnotationCreatorFactories.TransverseRebarFirst);
        TransverseViewSecondRebar = new PylonView(Settings, Repository, this,
                                                  AnnotationCreatorFactories.TransverseRebarSecond);
        TransverseViewThirdRebar = new PylonView(Settings, Repository, this,
                                                 AnnotationCreatorFactories.TransverseRebarThird);

        SkeletonSchedule = new PylonView(Settings, Repository, this);
        SkeletonByElemsSchedule = new PylonView(Settings, Repository, this);
        MaterialSchedule = new PylonView(Settings, Repository, this);
        SystemPartsSchedule = new PylonView(Settings, Repository, this);
        IfcPartsSchedule = new PylonView(Settings, Repository, this);

        LegendView = new PylonView(Settings, Repository, this);

        ElemsInfo = new PylonElemsInfo(Settings, Repository, this);
        RebarInfo = new PylonRebarInfo(Settings, Repository, this);
    }

    /// <summary>
    /// Создание листа, задание имени, поиск рамки и задание ей нужных габаритов
    /// </summary>
    public bool CreateSheet() {
        if(PylonViewSheet != null) {
            return false;
        }

        PylonViewSheet = ViewSheet.Create(Repository.Document, Settings.SheetSettings.SelectedTitleBlock.Id);
        PylonViewSheet.Name = Settings.SheetSettings.SheetPrefix + PylonKeyName + Settings.SheetSettings.SheetSuffix;

        var viewSheetGroupingParameter = PylonViewSheet.LookupParameter(Settings.ProjectSettings.DispatcherGroupingFirst);
        if(viewSheetGroupingParameter == null) {
        } else {
            viewSheetGroupingParameter.Set(ProjectSection);
        }

        FindTitleBlock();
        // Если пользователь выбрал создание основного или бокового вида каркаса, то нужна большая рамка А1
        var sels = Settings.SelectionSettings;
        if(sels.NeedWorkWithGeneralRebarView || sels.NeedWorkWithGeneralPerpendicularRebarView) {
            SetTitleBlockSize(Repository.Document, 1, 1);
        } else {
            SetTitleBlockSize(Repository.Document);
        }
        return true;
    }

    /// <summary>
    /// Поиск рамки на листе
    /// </summary>
    public bool FindTitleBlock() {
        // Ищем рамку листа

        if(new FilteredElementCollector(Repository.Document, PylonViewSheet.Id)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .WhereElementIsNotElementType()
            .FirstOrDefault() is not FamilyInstance titleBlock) { return false; }
        TitleBlock = titleBlock;
        return true;
    }


    /// <summary>
    /// Получает габариты рамки листа и записывает в параметры TitleBlockWidth и TitleBlockHeight
    /// </summary>
    internal void GetTitleBlockSize() {
        if(TitleBlock is null) {
            return;
        }

        // Получение габаритов рамки листа
        var boundingBoxXYZ = TitleBlock.get_BoundingBox(PylonViewSheet);
        TitleBlockWidth = -1 * boundingBoxXYZ.Min.X;
        TitleBlockHeight = boundingBoxXYZ.Max.Y;
    }


    /// <summary>
    /// Задает размеры рамки листа (по дефолту ставит А3) и запоминает
    /// </summary>
    internal void SetTitleBlockSize(Document doc, int sheetSize = 3, int sheetCoefficient = 1) {
        if(TitleBlock is null) {
            return;
        }
        // Пытаемся задать габарит листа
        var paramA = TitleBlock.LookupParameter(Settings.SheetSettings.SheetSize);
        var paramX = TitleBlock.LookupParameter(Settings.SheetSettings.SheetCoefficient);

        if(Settings.SheetSettings.CustomTitleBlockIsCheck) {
            sheetSize = int.Parse(Settings.SheetSettings.CustomSheetSizeValue);
            sheetCoefficient = int.Parse(Settings.SheetSettings.CustomSheetCoefficientValue);
        }

        if(paramA != null && paramX != null) {
            paramA.Set(sheetSize);
            paramX.Set(sheetCoefficient);
            doc.Regenerate();

            string format = string.Empty;
            if(sheetCoefficient > 1) {
                format = string.Format("А{0}х{1}", sheetSize, sheetCoefficient);
            } else {
                format = "А" + sheetSize;
            }
        }

        // Получение итоговых габаритов рамки листа
        GetTitleBlockSize();
    }


    /// <summary>
    /// Получает и сохраняет имена видов в соответствии с префиксами/суффиксами, которые указал пользователь
    /// </summary>
    public void GetViewNamesForWork() {
        GeneralView.ViewName = Settings.VerticalViewSettings.GeneralViewPrefix + PylonKeyName + Settings.VerticalViewSettings.GeneralViewSuffix;
        GeneralViewPerpendicular.ViewName = Settings.VerticalViewSettings.GeneralViewPerpendicularPrefix + PylonKeyName + Settings.VerticalViewSettings.GeneralViewPerpendicularSuffix;
        TransverseViewFirst.ViewName = Settings.TransverseViewSettings.TransverseViewFirstPrefix + PylonKeyName + Settings.TransverseViewSettings.TransverseViewFirstSuffix;
        TransverseViewSecond.ViewName = Settings.TransverseViewSettings.TransverseViewSecondPrefix + PylonKeyName + Settings.TransverseViewSettings.TransverseViewSecondSuffix;
        TransverseViewThird.ViewName = Settings.TransverseViewSettings.TransverseViewThirdPrefix + PylonKeyName + Settings.TransverseViewSettings.TransverseViewThirdSuffix;

        MaterialSchedule.ViewName = Settings.SchedulesSettings.MaterialSchedulePrefix + PylonKeyName + Settings.SchedulesSettings.MaterialScheduleSuffix;
        SystemPartsSchedule.ViewName = Settings.SchedulesSettings.SystemPartsSchedulePrefix + PylonKeyName + Settings.SchedulesSettings.SystemPartsScheduleSuffix;
        IfcPartsSchedule.ViewName = Settings.SchedulesSettings.IfcPartsSchedulePrefix + PylonKeyName + Settings.SchedulesSettings.IfcPartsScheduleSuffix;

        GeneralViewRebar.ViewName = Settings.VerticalViewSettings.GeneralRebarViewPrefix + PylonKeyName + Settings.VerticalViewSettings.GeneralRebarViewSuffix;
        GeneralViewPerpendicularRebar.ViewName = Settings.VerticalViewSettings.GeneralRebarViewPerpendicularPrefix + PylonKeyName + Settings.VerticalViewSettings.GeneralRebarViewPerpendicularSuffix;
        TransverseViewFirstRebar.ViewName = Settings.TransverseViewSettings.TransverseRebarViewFirstPrefix + PylonKeyName + Settings.TransverseViewSettings.TransverseRebarViewFirstSuffix;
        TransverseViewSecondRebar.ViewName = Settings.TransverseViewSettings.TransverseRebarViewSecondPrefix + PylonKeyName + Settings.TransverseViewSettings.TransverseRebarViewSecondSuffix;
        TransverseViewThirdRebar.ViewName = Settings.TransverseViewSettings.TransverseRebarViewThirdPrefix + PylonKeyName + Settings.TransverseViewSettings.TransverseRebarViewThirdSuffix;

        SkeletonSchedule.ViewName = Settings.SchedulesSettings.SkeletonSchedulePrefix + PylonKeyName + Settings.SchedulesSettings.SkeletonScheduleSuffix;
        SkeletonByElemsSchedule.ViewName = Settings.SchedulesSettings.SkeletonByElemsSchedulePrefix + PylonKeyName + Settings.SchedulesSettings.SkeletonByElemsScheduleSuffix;
    }


    /// <summary>
    /// Ищет и запоминает виды и видовые экраны через видовые экраны, размещенные на листе
    /// </summary>
    public void FindViewsNViewportsOnSheet() {
        foreach(ElementId id in PylonViewSheet.GetAllViewports()) {
            Viewport viewport = Repository.Document.GetElement(id) as Viewport;
            ViewSection viewSection = Repository.Document.GetElement(viewport.ViewId) as ViewSection;

            // Видовые экраны не разрезов нас не интересуют
            if(viewSection is null) { continue; }

            // Если вид не находили до этого в этом цикле и его имя равно нужному, то сохраняем
            // GeneralView
            if(GeneralView.ViewElement is null && viewSection.Name.Equals(GeneralView.ViewName)) {
                GeneralView.ViewElement = viewSection;
                GeneralView.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralView, viewport, true);
                continue;
            }

            // GeneralRebarView
            if(GeneralViewRebar.ViewElement is null && viewSection.Name.Equals(GeneralViewRebar.ViewName)) {
                GeneralViewRebar.ViewElement = viewSection;
                GeneralViewRebar.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralViewRebar, viewport, true);
                continue;
            }

            // GeneralViewPerpendicular
            if(GeneralViewPerpendicular.ViewElement is null && viewSection.Name.Equals(GeneralViewPerpendicular.ViewName)) {
                GeneralViewPerpendicular.ViewElement = viewSection;
                GeneralViewPerpendicular.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralViewPerpendicular, viewport, true);
                continue;
            }

            // GeneralRebarViewPerpendicular
            if(GeneralViewPerpendicularRebar.ViewElement is null && viewSection.Name.Equals(GeneralViewPerpendicularRebar.ViewName)) {
                GeneralViewPerpendicularRebar.ViewElement = viewSection;
                GeneralViewPerpendicularRebar.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralViewPerpendicularRebar, viewport, true);
                continue;
            }

            // TransverseViewFirst
            if(TransverseViewFirst.ViewElement is null && viewSection.Name.Equals(TransverseViewFirst.ViewName)) {
                TransverseViewFirst.ViewElement = viewSection;
                TransverseViewFirst.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewFirst, viewport, true);
                continue;
            }

            // TransverseViewSecond
            if(TransverseViewSecond.ViewElement is null && viewSection.Name.Equals(TransverseViewSecond.ViewName)) {
                TransverseViewSecond.ViewElement = viewSection;
                TransverseViewSecond.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewSecond, viewport, true);
                continue;
            }

            // TransverseViewThird
            if(TransverseViewThird.ViewElement is null && viewSection.Name.Equals(TransverseViewThird.ViewName)) {
                TransverseViewThird.ViewElement = viewSection;
                TransverseViewThird.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewThird, viewport, true);
                continue;
            }

            // TransverseRebarViewFirst
            if(TransverseViewFirstRebar.ViewElement is null && viewSection.Name.Equals(TransverseViewFirstRebar.ViewName)) {
                TransverseViewFirstRebar.ViewElement = viewSection;
                TransverseViewFirstRebar.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewFirstRebar, viewport, true);
                continue;
            }

            // TransverseRebarViewSecond
            if(TransverseViewSecondRebar.ViewElement is null && viewSection.Name.Equals(TransverseViewSecondRebar.ViewName)) {
                TransverseViewSecondRebar.ViewElement = viewSection;
                TransverseViewSecondRebar.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewSecondRebar, viewport, true);
                continue;
            }

            // TransverseRebarViewThird
            if(TransverseViewThirdRebar.ViewElement is null && viewSection.Name.Equals(TransverseViewThirdRebar.ViewName)) {
                TransverseViewThirdRebar.ViewElement = viewSection;
                TransverseViewThirdRebar.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewThirdRebar, viewport, true);
                continue;
            }
        }
    }


    /// <summary>
    /// Ищет и запоминает легенду, размещенную на листе
    /// </summary>
    public void FindNoteLegendOnSheet() {
        if(Settings.LegendsAndAnnotationsSettings.SelectedLegend is null) {
            return;
        }
        foreach(ElementId id in PylonViewSheet.GetAllViewports()) {
            Viewport viewportLegend = Repository.Document.GetElement(id) as Viewport;
            if(viewportLegend is null) { continue; }

            View viewLegend = Repository.Document.GetElement(viewportLegend.ViewId) as View;
            if(viewLegend is null) { continue; }

            if(LegendView.ViewElement is null && viewLegend.Name.Equals(Settings.LegendsAndAnnotationsSettings.SelectedLegend.Name)) {
                LegendView.ViewElement = viewLegend;
                LegendView.ViewportElement = viewportLegend;
                return;
            }
        }
    }


    /// <summary>
    /// Ищет и запоминает спеки и их видовые экраны через видовые экраны, размещенные на листе
    /// </summary>
    public void FindSchedulesNViewportsOnSheet() {
        foreach(ElementId id in PylonViewSheet.GetDependentElements(new ElementClassFilter(typeof(ScheduleSheetInstance)))) {
            ScheduleSheetInstance viewport = Repository.Document.GetElement(id) as ScheduleSheetInstance;
            if(viewport is null) { continue; }

            ViewSchedule viewSchedule = Repository.Document.GetElement(viewport.ScheduleId) as ViewSchedule;
            if(viewSchedule is null) { continue; }

            // Если вид не находили до этого в этом цикле и его имя равно нужному, то сохраняем
            // MaterialSchedule
            if(MaterialSchedule.ViewElement is null && viewSchedule.Name.Equals(MaterialSchedule.ViewName)) {
                MaterialSchedule.ViewElement = viewSchedule;
                MaterialSchedule.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutScheduleSheetInstance(MaterialSchedule, viewport);
                continue;
            }

            // SystemPartsSchedule
            if(SystemPartsSchedule.ViewElement is null && viewSchedule.Name.Equals(SystemPartsSchedule.ViewName)) {
                SystemPartsSchedule.ViewElement = viewSchedule;
                SystemPartsSchedule.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutScheduleSheetInstance(SystemPartsSchedule, viewport);
                continue;
            }

            // IFCPartsSchedule
            if(IfcPartsSchedule.ViewElement is null && viewSchedule.Name.Equals(IfcPartsSchedule.ViewName)) {
                IfcPartsSchedule.ViewElement = viewSchedule;
                IfcPartsSchedule.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutScheduleSheetInstance(IfcPartsSchedule, viewport);
                continue;
            }

            // SkeletonSchedule
            if(SkeletonSchedule.ViewElement is null && viewSchedule.Name.Equals(SkeletonSchedule.ViewName)) {
                SkeletonSchedule.ViewElement = viewSchedule;
                SkeletonSchedule.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutScheduleSheetInstance(SkeletonSchedule, viewport);
                continue;
            }

            // SkeletonByElemsSchedule
            if(SkeletonByElemsSchedule.ViewElement is null && viewSchedule.Name.Equals(SkeletonByElemsSchedule.ViewName)) {
                SkeletonByElemsSchedule.ViewElement = viewSchedule;
                SkeletonByElemsSchedule.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutScheduleSheetInstance(SkeletonByElemsSchedule, viewport);
                continue;
            }
        }
    }


    /// <summary>
    /// Получение и сохранение информации о центре и габаритах видового экрана
    /// </summary>
    public void GetInfoAboutViewport(PylonView pylonView, Viewport viewport, bool hideUnnecessaryCategories) {
        // Скрывать элементы перечисленных ниже категорий может быть необходимо в связи с тем, что они сильно
        // расширяют габариты видового экрана на листе, при этом не являясь существенными элементами на виде
        var view = pylonView.ViewElement;
        if(hideUnnecessaryCategories) {
            BuiltInCategory[] categoriesToHide = {
                BuiltInCategory.OST_GenericAnnotation,
                BuiltInCategory.OST_RebarTags,
                BuiltInCategory.OST_Viewers
            };
            var multiCategoryFilter = new LogicalOrFilter(
                categoriesToHide
                    .Select<BuiltInCategory, ElementFilter>(bic => new ElementCategoryFilter(bic))
                    .ToList()
            );
            var elemIdsForHide = new FilteredElementCollector(Repository.Document, view.Id)
                .WherePasses(multiCategoryFilter)
                .ToElementIds();
            view.HideElementsTemporary(elemIdsForHide);
        }

        var viewportCenter = viewport.GetBoxCenter();
        var viewportOutline = viewport.GetBoxOutline();
        double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
        double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

        pylonView.ViewportCenter = viewportCenter;
        pylonView.ViewportHalfWidth = viewportHalfWidth;
        pylonView.ViewportHalfHeight = viewportHalfHeight;

        if(hideUnnecessaryCategories) {
            view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
        }
    }


    /// <summary>
    /// Получение и сохранение информации о центре и габаритах видового экрана спек
    /// </summary>
    public void GetInfoAboutScheduleSheetInstance(PylonView pylonView, ScheduleSheetInstance scheduleSheetInstance) {
        // Получение габаритов видового экрана спецификации
        var viewportCenter = scheduleSheetInstance.Point;
        var boundingBoxXYZ = scheduleSheetInstance.get_BoundingBox(pylonView.SheetInfo.PylonViewSheet);
        double scheduleHalfWidth = boundingBoxXYZ.Max.X / 2;
        double scheduleHalfHeight = -boundingBoxXYZ.Min.Y / 2;     // Создается так, что верхний левый угол спеки в нижнем правом углу рамки

        pylonView.ViewportCenter = viewportCenter;
        pylonView.ViewportHalfWidth = scheduleHalfWidth;
        pylonView.ViewportHalfHeight = scheduleHalfHeight;
    }
}
