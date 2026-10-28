using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
/// <summary>
/// Модуль вида в плане: создает план с нуля на уровне элемента привязки или дублированием исходного вида
/// и размещает его на листе
/// </summary>
internal class PlanViewVM : SheetComponentVM {
    // При разработке документации пользователям необходимо иметь возможность задать одинаковый Номер вида
    // для нескольких компонентов одного листа (значение задается одинаковое, но за счет разного типа вида в заголовке
    // видового экрана на листе отображается по разному).
    // По умолчанию в Revit это не допускается, поэтому пользователи используют регламентированные стандартами невидимые
    // символы для различных типов видов. Для отличия Номера вида у планов регламентирован невидимый символ
    // указанный ниже (в Юникоде это Left-to-Right Mark (LRM))
    private const string _uniqueViewportNumberKey = "\u200E";

    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewPlan _viewTemplate;
    private string _viewCount;
    private SelectElemParamVM _selectedSelectElemParam;
    private ViewPlan _viewInstance;
    private bool _isDuplicate;
    private ViewPlan _referenceView;
    private FiltrationComboBoxFilterListVM _referenceViewFilter;

    private FiltrationComboBoxFilterListVM _viewportTypeFilter;
    private FiltrationComboBoxFilterListVM _viewFamilyTypeFilter;
    private FiltrationComboBoxFilterListVM _viewTemplateFilter;
    private bool _viewTemplateRemoveAfterCreation = false;

    // Отступ по горизонтали от левого края рамки, 20 мм, чтобы вид попал внутрь рамки (во внутренних единицах Revit)
    private readonly double _titleBlockFrameLeftOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Отступ по вертикали от верхнего края рамки, 15 мм, чтобы вид попал внутрь рамки (во внутренних единицах Revit)
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    public PlanViewVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        ChangeCreationModeCommand = RelayCommand.Create(ChangeCreationMode);
    }

    /// <summary>
    /// Команда смены способа создания вида (с нуля / дублированием), вызывается тумблером
    /// </summary>
    public ICommand ChangeCreationModeCommand { get; }

    [Required(ErrorMessage = "Validation.ViewNameIsEmpty")]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\];~]+$", ErrorMessage = "Validation.ViewNameIsNotCorrect")]
    public string ViewNameFormula {
        get => _viewNameFormula;
        set => RaiseAndSetIfChanged(ref _viewNameFormula, value);
    }

    /// <summary>
    /// Имя вида, вычисленное по формуле
    /// </summary>
    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    /// <summary>
    /// Способ создания вида: false - с нуля по привязке к объекту, true - дублированием исходного вида
    /// </summary>
    public bool IsDuplicate {
        get => _isDuplicate;
        set => RaiseAndSetIfChanged(ref _isDuplicate, value);
    }

    /// <summary>
    /// Вид создается с нуля - доступен выбор привязки к объекту
    /// </summary>
    public bool IsCreatingFromScratch => !IsDuplicate;

    /// <summary>
    /// Исходный вид для дублирования
    /// </summary>
    [RequiredIf(nameof(IsDuplicate), ErrorMessage = "Validation.ReferenceViewIsNull")]
    public ViewPlan ReferenceView {
        get => _referenceView;
        set => RaiseAndSetIfChanged(ref _referenceView, value);
    }

    public FiltrationComboBoxFilterListVM ReferenceViewFilter {
        get => _referenceViewFilter;
        set => RaiseAndSetIfChanged(ref _referenceViewFilter, value);
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

    // Привязка к объекту нужна только при создании вида с нуля
    [RequiredIf(nameof(IsDuplicate), false, ErrorMessage = "Validation.SelectedSelectElemParamIsNull")]
    [ChildHasErrors(
        ConditionPropertyName = nameof(IsDuplicate),
        ConditionValue = false,
        ErrorMessage = "Validation.SelectElemParamSelectedElemIsNull")]
    public SelectElemParamVM SelectedSelectElemParam {
        get => _selectedSelectElemParam;
        set => RaiseAndSetIfChanged(ref _selectedSelectElemParam, value);
    }

    /// <summary>
    /// Уровень плана, который создаст модуль: в режиме дублирования - уровень исходного вида,
    /// при создании с нуля - уровень элемента привязки. Null, если определить нельзя
    /// </summary>
    public ElementId GetPlanLevelId() {
        var levelId = IsDuplicate
            ? ReferenceView?.GenLevel?.Id
            : SelectedSelectElemParam?.SelectedElem?.LevelId;
        return levelId is null || levelId == ElementId.InvalidElementId ? null : levelId;
    }

    /// <summary>
    /// Семейство вида плана, который создаст модуль: в режиме дублирования - семейство исходного вида
    /// (типоразмер другого семейства ему не назначается), при создании с нуля - семейство выбранного типоразмера
    /// </summary>
    public ViewFamily? GetPlanViewFamily() {
        return IsDuplicate
            ? Repository.GetViewFamily(ReferenceView)
            : ViewFamilyType?.ViewFamily;
    }

    /// <summary>
    /// Найденный или созданный вид в модели
    /// </summary>
    public ViewPlan ViewInstance {
        get => _viewInstance;
        set => RaiseAndSetIfChanged(ref _viewInstance, value);
    }

    /// <summary>
    /// Находит или создает вид, размещает его на листе и заполняет дополнительные параметры видового экрана
    /// </summary>
    public override void Process(bool processDependent = false) {
        ViewInstance = Create();
        var viewPort = Place(ViewInstance);
        SetCustomParams(viewPort);
    }

    private void ChangeCreationMode() {
        RaisePropertyChanged(nameof(IsCreatingFromScratch));
        // От способа создания зависит, какие поля обязательны (исходный вид или привязка к объекту)
        ValidateAllProperties();
    }

    /// <summary>
    /// Возвращает существующий вид с таким именем или создает новый. Null, если вид создать не удалось
    /// </summary>
    public ViewPlan Create() {
        // Ищется план того же семейства, что создаст модуль: план с таким же именем другого типа
        // (например, план потолка) Revit допускает, и его использовать нельзя
        var view = Repository.FindViewByName<ViewPlan>(ViewName, GetPlanViewFamily());

        if(view is null) {
            try {
                if(IsDuplicate) {
                    if(ReferenceView is null || !ReferenceView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
                        return null;
                    }
                    // Копирование с детализацией, секущий диапазон остается как у исходного вида
                    var viewId = ReferenceView.Duplicate(ViewDuplicateOption.WithDetailing);
                    view = Repository.Document.GetElement(viewId) as ViewPlan;
                    if(view is null) {
                        return null;
                    }
                    view.Name = ViewName;
                    SetViewFamilyType(view);
                    SetViewTemplate(view);
                } else {
                    var selectedElem = SelectedSelectElemParam.SelectedElem;
                    var levelId = selectedElem.LevelId;
                    if(levelId is null) {
                        return null;
                    }
                    view = ViewPlan.Create(Repository.Document, ViewFamilyType.Id, levelId);
                    view.Name = ViewName;
                    SetViewTemplate(view);
                    SetViewRange(view, selectedElem);
                }

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

    /// <summary>
    /// Задает секущий диапазон вида, созданного с нуля, относительно отметки элемента привязки
    /// </summary>
    private void SetViewRange(ViewPlan view, Element selectedElem) {
        double elementFromLevelOffset = selectedElem.GetParamValue<double>(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

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
    }

    /// <summary>
    /// Назначает типоразмер вида. Revit позволяет сменить типоразмер только в пределах одного семейства видов,
    /// поэтому если исходный вид из другого семейства (например, план потолка), типоразмер остается как у него
    /// </summary>
    private void SetViewFamilyType(ViewPlan view) {
        if(ViewFamilyType is null
            || Repository.Document.GetElement(view.GetTypeId()) is not ViewFamilyType currentType
            || currentType.ViewFamily != ViewFamilyType.ViewFamily) {
            return;
        }
        view.ChangeTypeId(ViewFamilyType.Id);
    }

    /// <summary>
    /// Назначает шаблон вида, если он подходит виду (например, шаблон плана этажа не подходит плану потолка)
    /// </summary>
    private void SetViewTemplate(View view) {
        if(ViewTemplate != null && view.IsValidViewTemplate(ViewTemplate.Id)) {
            view.ViewTemplateId = ViewTemplate.Id;
        }
    }

    /// <summary>
    /// Размещает вид на листе: первый план - в левом верхнем углу рамки, следующие - в ряд над рамкой.
    /// Номер видового экрана помечается невидимым символом планов
    /// </summary>
    /// <returns>Созданный видовой экран или null, если вид разместить нельзя</returns>
    public Viewport Place(ViewPlan view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance != null
            && view != null
            && Viewport.CanAddViewToSheet(Repository.Document, sheetInstance.Id, view.Id)) {

            // Получение габаритов рамки листа
            if(Repository.GetTitleBlock(sheetInstance) is not FamilyInstance titleBlock) {
                return null;
            }
            var boundingBoxXYZ = titleBlock.get_BoundingBox(sheetInstance);
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
