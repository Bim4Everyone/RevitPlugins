using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
using RevitPylonDocumentation.ViewModels;

using Document = Autodesk.Revit.DB.Document;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonSheetInfo : BaseViewModel {
    private bool _isCheck = false;

    internal PylonSheetInfo(MainViewModel mvm, RevitRepository repository, string projectSection, string pylonKeyName) {
        ViewModel = mvm;
        Repository = repository;
        PylonKeyName = pylonKeyName;
        ProjectSection = projectSection;

        Manager = new PylonSheetInfoManager(ViewModel, Repository, this);

        GeneralView = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.General);
        GeneralViewPerpendicular = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.GeneralPerp);
        GeneralRebarView = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.GeneralRebar);
        GeneralRebarViewPerpendicular = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.GeneralRebarPerp);
        TransverseViewFirst = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.TransverseFirst);
        TransverseViewSecond = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.TransverseSecond);
        TransverseViewThird = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.TransverseThird);
        TransverseRebarViewFirst = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.TransverseRebarFirst);
        TransverseRebarViewSecond = new PylonView(ViewModel, Repository, this, DimensionCreatorFactories.TransverseRebarSecond);

        RebarSchedule = new PylonView(ViewModel, Repository, this);
        SkeletonSchedule = new PylonView(ViewModel, Repository, this);
        SkeletonByElemsSchedule = new PylonView(ViewModel, Repository, this);
        MaterialSchedule = new PylonView(ViewModel, Repository, this);
        SystemPartsSchedule = new PylonView(ViewModel, Repository, this);
        IfcPartsSchedule = new PylonView(ViewModel, Repository, this);

        LegendView = new PylonView(ViewModel, Repository, this);
        RebarNodeView = new PylonView(ViewModel, Repository, this);

        RebarInfo = new PylonRebarInfo(mvm, Repository, this);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfoManager Manager { get; set; }
    internal PylonRebarInfo RebarInfo { get; set; }


    public bool IsCheck {
        get => _isCheck;
        set => RaiseAndSetIfChanged(ref _isCheck, value);
    }

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
    public PylonView GeneralRebarView { get; set; }
    public PylonView GeneralViewPerpendicular { get; set; }
    public PylonView GeneralRebarViewPerpendicular { get; set; }
    public PylonView TransverseViewFirst { get; set; }
    public PylonView TransverseViewSecond { get; set; }
    public PylonView TransverseViewThird { get; set; }
    public PylonView TransverseRebarViewFirst { get; set; }
    public PylonView TransverseRebarViewSecond { get; set; }



    // Видовые экраны спецификаций
    public PylonView RebarSchedule { get; set; }
    public PylonView MaterialSchedule { get; set; }
    public PylonView SystemPartsSchedule { get; set; }
    public PylonView IfcPartsSchedule { get; set; }
    public PylonView SkeletonSchedule { get; set; }
    public PylonView SkeletonByElemsSchedule { get; set; }


    // Легенда примечаний
    public PylonView LegendView { get; set; }

    // Легенда узла армирования
    public PylonView RebarNodeView { get; set; }



    /// <summary>
    /// Создание листа, задание имени, поиск рамки и задание ей нужных габаритов
    /// </summary>
    public bool CreateSheet() {
        if(PylonViewSheet != null) {
            return false;
        }

        PylonViewSheet = ViewSheet.Create(Repository.Document, ViewModel.SelectedTitleBlock.Id);
        PylonViewSheet.Name = ViewModel.ProjectSettings.SheetPrefix + PylonKeyName + ViewModel.ProjectSettings.SheetSuffix;

        var viewSheetGroupingParameter = PylonViewSheet.LookupParameter(ViewModel.ProjectSettings.DispatcherGroupingFirst);
        if(viewSheetGroupingParameter == null) {
        } else {
            viewSheetGroupingParameter.Set(ViewModel.SelectedProjectSection);
        }

        FindTitleBlock();
        // Если пользователь выбрал создание основного или бокового вида каркаса, то нужна большая рамка А1
        var sels = ViewModel.SelectionSettings;
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
        var paramA = TitleBlock.LookupParameter(ViewModel.ProjectSettings.SheetSize);
        var paramX = TitleBlock.LookupParameter(ViewModel.ProjectSettings.SheetCoefficient);

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
        GeneralView.ViewName = ViewModel.ViewSectionSettings.GeneralViewPrefix + PylonKeyName + ViewModel.ViewSectionSettings.GeneralViewSuffix;
        GeneralViewPerpendicular.ViewName = ViewModel.ViewSectionSettings.GeneralViewPerpendicularPrefix + PylonKeyName + ViewModel.ViewSectionSettings.GeneralViewPerpendicularSuffix;
        TransverseViewFirst.ViewName = ViewModel.ViewSectionSettings.TransverseViewFirstPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseViewFirstSuffix;
        TransverseViewSecond.ViewName = ViewModel.ViewSectionSettings.TransverseViewSecondPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseViewSecondSuffix;
        TransverseViewThird.ViewName = ViewModel.ViewSectionSettings.TransverseViewThirdPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseViewThirdSuffix;

        RebarSchedule.ViewName = ViewModel.SchedulesSettings.RebarSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.RebarScheduleSuffix;
        MaterialSchedule.ViewName = ViewModel.SchedulesSettings.MaterialSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.MaterialScheduleSuffix;
        SystemPartsSchedule.ViewName = ViewModel.SchedulesSettings.SystemPartsSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.SystemPartsScheduleSuffix;
        IfcPartsSchedule.ViewName = ViewModel.SchedulesSettings.IfcPartsSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.IfcPartsScheduleSuffix;

        GeneralRebarView.ViewName = ViewModel.ViewSectionSettings.GeneralRebarViewPrefix + PylonKeyName + ViewModel.ViewSectionSettings.GeneralRebarViewSuffix;
        GeneralRebarViewPerpendicular.ViewName = ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularPrefix + PylonKeyName + ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularSuffix;
        TransverseRebarViewFirst.ViewName = ViewModel.ViewSectionSettings.TransverseRebarViewFirstPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseRebarViewFirstSuffix;
        TransverseRebarViewSecond.ViewName = ViewModel.ViewSectionSettings.TransverseRebarViewSecondPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseRebarViewSecondSuffix;

        SkeletonSchedule.ViewName = ViewModel.SchedulesSettings.SkeletonSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.SkeletonScheduleSuffix;
        SkeletonByElemsSchedule.ViewName = ViewModel.SchedulesSettings.SkeletonByElemsSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.SkeletonByElemsScheduleSuffix;
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
                GetInfoAboutViewport(GeneralView, viewport);
                continue;
            }

            // GeneralRebarView
            if(GeneralRebarView.ViewElement is null && viewSection.Name.Equals(GeneralRebarView.ViewName)) {
                GeneralRebarView.ViewElement = viewSection;
                GeneralRebarView.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralRebarView, viewport);
                continue;
            }

            // GeneralViewPerpendicular
            if(GeneralViewPerpendicular.ViewElement is null && viewSection.Name.Equals(GeneralViewPerpendicular.ViewName)) {
                GeneralViewPerpendicular.ViewElement = viewSection;
                GeneralViewPerpendicular.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralViewPerpendicular, viewport);
                continue;
            }

            // GeneralRebarViewPerpendicular
            if(GeneralRebarViewPerpendicular.ViewElement is null && viewSection.Name.Equals(GeneralRebarViewPerpendicular.ViewName)) {
                GeneralRebarViewPerpendicular.ViewElement = viewSection;
                GeneralRebarViewPerpendicular.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(GeneralRebarViewPerpendicular, viewport);
                continue;
            }

            // TransverseViewFirst
            if(TransverseViewFirst.ViewElement is null && viewSection.Name.Equals(TransverseViewFirst.ViewName)) {
                TransverseViewFirst.ViewElement = viewSection;
                TransverseViewFirst.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewFirst, viewport);
                continue;
            }

            // TransverseViewSecond
            if(TransverseViewSecond.ViewElement is null && viewSection.Name.Equals(TransverseViewSecond.ViewName)) {
                TransverseViewSecond.ViewElement = viewSection;
                TransverseViewSecond.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewSecond, viewport);
                continue;
            }

            // TransverseViewThird
            if(TransverseViewThird.ViewElement is null && viewSection.Name.Equals(TransverseViewThird.ViewName)) {
                TransverseViewThird.ViewElement = viewSection;
                TransverseViewThird.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseViewThird, viewport);
                continue;
            }

            // TransverseRebarViewFirst
            if(TransverseRebarViewFirst.ViewElement is null && viewSection.Name.Equals(TransverseRebarViewFirst.ViewName)) {
                TransverseRebarViewFirst.ViewElement = viewSection;
                TransverseRebarViewFirst.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseRebarViewFirst, viewport);
                continue;
            }

            // TransverseRebarViewSecond
            if(TransverseRebarViewSecond.ViewElement is null && viewSection.Name.Equals(TransverseRebarViewSecond.ViewName)) {
                TransverseRebarViewSecond.ViewElement = viewSection;
                TransverseRebarViewSecond.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutViewport(TransverseRebarViewSecond, viewport);
                continue;
            }
        }
    }


    /// <summary>
    /// Ищет и запоминает легенду, размещенную на листе
    /// </summary>
    public void FindNoteLegendOnSheet() {
        if(ViewModel.SelectedLegend is null) {
            return;
        }
        foreach(ElementId id in PylonViewSheet.GetAllViewports()) {
            Viewport viewportLegend = Repository.Document.GetElement(id) as Viewport;
            if(viewportLegend is null) { continue; }

            View viewLegend = Repository.Document.GetElement(viewportLegend.ViewId) as View;
            if(viewLegend is null) { continue; }

            if(LegendView.ViewElement is null && viewLegend.Name.Equals(ViewModel.SelectedLegend.Name)) {
                LegendView.ViewElement = viewLegend;
                LegendView.ViewportElement = viewportLegend;
                return;
            }
        }
    }


    /// <summary>
    /// Ищет и запоминает легенду узла армирования, размещенную на листе
    /// </summary>
    public void FindRebarLegendNodeOnSheet() {
        if(ViewModel.SelectedRebarNode is null) {
            return;
        }
        foreach(ElementId id in PylonViewSheet.GetAllViewports()) {
            Viewport viewportLegend = Repository.Document.GetElement(id) as Viewport;
            if(viewportLegend is null) { continue; }

            View viewLegend = Repository.Document.GetElement(viewportLegend.ViewId) as View;
            if(viewLegend is null) { continue; }

            if(RebarNodeView.ViewElement is null && viewLegend.Name.Equals(ViewModel.SelectedRebarNode.Name)) {
                RebarNodeView.ViewElement = viewLegend;
                RebarNodeView.ViewportElement = viewportLegend;
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
            // RebarSchedule
            if(RebarSchedule.ViewElement is null && viewSchedule.Name.Equals(RebarSchedule.ViewName)) {
                RebarSchedule.ViewElement = viewSchedule;
                RebarSchedule.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                GetInfoAboutScheduleSheetInstance(RebarSchedule, viewport);
                continue;
            }

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
    public void GetInfoAboutViewport(PylonView pylonView, Viewport viewport) {
        var viewportCenter = viewport.GetBoxCenter();
        var viewportOutline = viewport.GetBoxOutline();
        double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
        double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

        pylonView.ViewportCenter = viewportCenter;
        pylonView.ViewportHalfWidth = viewportHalfWidth;
        pylonView.ViewportHalfHeight = viewportHalfHeight;
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
