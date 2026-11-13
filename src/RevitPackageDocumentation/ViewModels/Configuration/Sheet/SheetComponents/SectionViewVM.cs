using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

/// <summary>
/// Модуль сечений: создает указанное количество разрезов через элемент привязки
/// и размещает их в ряд под рамкой листа
/// </summary>
internal class SectionViewVM : SheetComponentVM {
    // При разработке документации пользователям необходимо иметь возможность задать одинаковый Номер вида
    // для нескольких компонентов одного листа (значение задается одинаковое, но за счет разного типа вида в заголовке
    // видового экрана на листе отображается по разному).
    // По умолчанию в Revit это не допускается, поэтому пользователи используют регламентированные стандартами невидимые
    // символы для различных типов видов. Для отличия Номера вида у сечений регламентирован невидимый символ
    // указанный ниже (в Юникоде это Right-to-Left Mark (RLM))
    private const string _uniqueViewportNumberKey = "\u200F";

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
    private bool _viewTemplateRemoveAfterCreation = false;

    // Размеры задаются в мм и переводятся во внутренние единицы Revit

    // Расстояние до дальней секущей плоскости сечения
    private readonly double _viewDepth = UnitUtilsHelper.ConvertToInternalValue(3000);

    // Ширина подрезки вида
    private readonly double _viewWidth = UnitUtilsHelper.ConvertToInternalValue(3000);

    // Высота подрезки вида
    private readonly double _viewHeight = UnitUtilsHelper.ConvertToInternalValue(1500);

    // Отступ между сечениями в модели
    private readonly double _viewsOffset = UnitUtilsHelper.ConvertToInternalValue(1500);

    public SectionViewVM(
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
    public ViewSection ViewTemplate {
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

    /// <summary>
    /// Создает (или находит) ViewCount сечений с именами ИмяВида_1..N и размещает их на листе
    /// </summary>
    public override void Process(bool processDependent = false) {
        int.TryParse(ViewCount, out int viewCountAsInt);

        for(int i = 1; i <= viewCountAsInt; i++) {
            var view = Create(i);
            var viewPort = Place(view, i);
            SetCustomParams(viewPort);
        }
    }

    /// <summary>
    /// Возвращает существующее сечение ИмяВида_number или создает новое через центр элемента привязки,
    /// со сдвигом вдоль оси X на number - 1 шагов. Null, если сечение создать не удалось
    /// </summary>
    public ViewSection Create(int number) {
        var viewName = GetNumberedViewName(number);
        // Ищется вид того же семейства: с таким же именем может быть, например, узел или план
        var view = Repository.FindViewByName<ViewSection>(viewName, ViewFamilyType?.ViewFamily);

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

            // Снимаем шаблон вида, если запросил пользователь
            if(ViewTemplateRemoveAfterCreation) {
                view.ViewTemplateId = ElementId.InvalidElementId;
            }
        } catch(System.Exception) { }
        return view;
    }

    /// <summary>
    /// Имя вида с порядковым номером: ИмяВида_number
    /// </summary>
    public string GetNumberedViewName(int number) => $"{ViewName}_{number}";

    /// <summary>
    /// Размещает сечение под рамкой листа правее последнего размещенного сечения.
    /// Номер видового экрана помечается невидимым символом сечений
    /// </summary>
    /// <returns>Созданный видовой экран или null, если вид разместить нельзя</returns>
    public Viewport Place(ViewSection view, int number) {
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
            double titleBlockMinY = boundingBoxXYZ.Min.Y;

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
