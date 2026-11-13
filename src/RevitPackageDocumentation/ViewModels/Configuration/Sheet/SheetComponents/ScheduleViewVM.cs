using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.ScheduleFilters;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

/// <summary>
/// Модуль спецификации: дублирует исходную спецификацию, заменяет ее фильтры фильтрами модуля
/// и размещает на листе слева от рамки по столбцам
/// </summary>
internal class ScheduleViewVM : SheetComponentVM {
    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private string _viewColumn;
    private string _viewCount;
    private ViewSchedule _referenceSpec;
    private ScheduleFilterListVM _scheduleFilterList;
    private ScheduleSheetInstance _viewportInstance;

    private FiltrationComboBoxFilterListVM _referenceSpecFilter;

    // Смещения задаются в мм и переводятся во внутренние единицы Revit

    // Смещение по горизонтали влево от начала координат листа для первого столбца спецификаций
    private readonly double _specViewportRightOffset = UnitUtilsHelper.ConvertToInternalValue(0.77);

    // Смещение по вертикали вниз от верха рамки для первой спецификации столбца
    private readonly double _specViewportTopOffset = UnitUtilsHelper.ConvertToInternalValue(12);

    // Смещение по вертикали для спецификаций одного столбца,
    // требуемое для их корректного взаимного размещения на листе (в случае наличия маленькой шапки спецификации)
    private readonly double _scheduleTopOffsetSmall = UnitUtilsHelper.ConvertToInternalValue(2.117);

    public ScheduleViewVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        ScheduleFilterList = new ScheduleFilterListVM(this, stringParamSetService, localizationService);

        SelectReferenceSpecCommand = RelayCommand.Create(SelectReferenceSpec);
    }

    /// <summary>
    /// Команда выбора исходной спецификации: фильтры модуля заполняются фильтрами выбранной спецификации
    /// </summary>
    public ICommand SelectReferenceSpecCommand { get; }

    [Required(ErrorMessage = "Validation.ViewNameIsEmpty")]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\];~]+$", ErrorMessage = "Validation.ViewNameIsNotCorrect")]
    public string ViewNameFormula {
        get => _viewNameFormula;
        set => RaiseAndSetIfChanged(ref _viewNameFormula, value);
    }

    /// <summary>
    /// Имя итоговой спецификации, вычисленное по формуле
    /// </summary>
    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    /// <summary>
    /// Исходная спецификация для дублирования
    /// </summary>
    [Required(ErrorMessage = "Validation.ReferenceSpecIsNull")]
    public ViewSchedule ReferenceSpec {
        get => _referenceSpec;
        set => RaiseAndSetIfChanged(ref _referenceSpec, value);
    }

    public FiltrationComboBoxFilterListVM ReferenceSpecFilter {
        get => _referenceSpecFilter;
        set => RaiseAndSetIfChanged(ref _referenceSpecFilter, value);
    }

    /// <summary>
    /// Номер столбца размещения на листе: спецификации одного столбца размещаются друг под другом
    /// </summary>
    [PositiveInteger(ErrorMessage = "Validation.ViewColumnIsNotCorrect")]
    public string ViewColumn {
        get => _viewColumn;
        set => RaiseAndSetIfChanged(ref _viewColumn, value);
    }

    [PositiveInteger(ErrorMessage = "Validation.ViewCountIsNotCorrect")]
    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    [ChildHasErrors(ErrorMessage = "Validation.ScheduleFiltersIsNotCorrect")]
    public ScheduleFilterListVM ScheduleFilterList {
        get => _scheduleFilterList;
        set => RaiseAndSetIfChanged(ref _scheduleFilterList, value);
    }

    /// <summary>
    /// Размещенная на листе спецификация
    /// </summary>
    public ScheduleSheetInstance ViewportInstance {
        get => _viewportInstance;
        set => RaiseAndSetIfChanged(ref _viewportInstance, value);
    }

    private void SelectReferenceSpec() {
        ScheduleFilterList.SetSchedule(ReferenceSpec);
    }

    /// <summary>
    /// Находит или создает спецификацию, размещает ее на листе и заполняет дополнительные параметры
    /// </summary>
    public override void Process(bool processDependent = false) {
        var view = Create();
        ViewportInstance = Place(view);
        SetCustomParams(view);
    }

    /// <summary>
    /// Возвращает существующую спецификацию с таким именем или дублирует исходную и заменяет ее фильтры.
    /// Null, если спецификацию создать не удалось
    /// </summary>
    public ViewSchedule Create() {
        // Поиск по текущему состоянию документа: находит и спецификации, созданные в этом запуске
        var view = Repository.FindScheduleByName(ViewName);
        if(view != null) {
            return view;
        }

        if(ReferenceSpec is null || !ReferenceSpec.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return null;
        }

        try {
            var scheduleId = ReferenceSpec.Duplicate(ViewDuplicateOption.Duplicate);
            view = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(view is null) {
                return null;
            }
            view.Name = ViewName;

            var definition = view.Definition;

            // Удаляем каждый фильтр
            ClearAllFilters(definition);

            // Добавляем указанные пользователем фильтры
            ApplyFilters(definition);
        } catch(System.Exception) { }
        return view;
    }

    /// <summary>
    /// Размещает спецификацию слева от рамки: первая спецификация столбца - у верха рамки,
    /// следующие спецификации столбца - под предыдущей, новый столбец - левее уже размещенных
    /// </summary>
    /// <returns>Размещенная спецификация (или уже имеющаяся на листе) либо null</returns>
    public ScheduleSheetInstance Place(ViewSchedule view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance is null || view is null) {
            return null;
        }

        // Если видовой экран спецификации уже есть на листе, то не размещаем повторно
        if(Repository.GetScheduleSheetInstances(sheetInstance)
            .FirstOrDefault(s => s.ScheduleId == view.Id) is ScheduleSheetInstance scheduleSheetInstance) {
            return scheduleSheetInstance;
        }

        // Получение габаритов рамки листа
        if(Repository.GetTitleBlock(sheetInstance) is not FamilyInstance titleBlock) {
            return null;
        }
        var titleBlockBB = titleBlock.get_BoundingBox(sheetInstance);
        double titleBlockHeight = titleBlockBB.Max.Y - titleBlockBB.Min.Y;

        // Изначально размещаем в Zero
        // Точка вставки у спеки в верхнем левом углу спеки
        scheduleSheetInstance = ScheduleSheetInstance.Create(Repository.Document, sheetInstance.Id, view.Id, XYZ.Zero);

        var viewportBB = scheduleSheetInstance.get_BoundingBox(sheetInstance);
        double viewportWidth = viewportBB.Max.X - viewportBB.Min.X;
        double viewportHeight = viewportBB.Max.Y - viewportBB.Min.Y;

        // Значение по умолчанию = как для первой
        XYZ ptForPlace = new XYZ(
            -viewportWidth - _specViewportRightOffset,
            titleBlockHeight - _specViewportTopOffset,
            0);

        // Размещенные спеки на листе
        var placedScheduleComponents = Sheet.SheetComponents
            .OfType<ScheduleViewVM>()
            .Where(c => c.ViewportInstance is not null)
            .ToList();

        // Если на листе еще не было размещено спек
        if(placedScheduleComponents.Count == 0) {
            // Присваиваем новую точку
            scheduleSheetInstance.Point = ptForPlace;
            return scheduleSheetInstance;
        }

        // Размещенные спеки на листе из этой колонки
        var currentColumn = placedScheduleComponents.Where(c => c.ViewColumn.Equals(ViewColumn)).ToList();

        if(currentColumn.Count > 0) {
            // Если в этой колонке есть спеки - min последней
            var prevViewport = currentColumn
                .Select(v => v.ViewportInstance)
                .OrderBy(v => v.get_BoundingBox(sheetInstance).Min.Y)
                .First();
            ptForPlace = new XYZ(
                prevViewport.Point.X,
                prevViewport.get_BoundingBox(sheetInstance).Min.Y + _scheduleTopOffsetSmall,
                0);
        } else {
            // Если в этой колонки нет спек - х = min х предыдущей колонки
            ptForPlace = new XYZ(placedScheduleComponents
                .Select(c => c.ViewportInstance.get_BoundingBox(sheetInstance).Min)
                .OrderBy(pt => pt.X)
                .First()
                .X - viewportWidth,
                ptForPlace.Y,
                ptForPlace.Z);
        }
        // Присваиваем новую точку
        scheduleSheetInstance.Point = ptForPlace;
        return scheduleSheetInstance;
    }

    private void ClearAllFilters(ScheduleDefinition definition) {
        for(int i = definition.GetFilters().Count - 1; i >= 0; i--) {
            definition.RemoveFilter(i);
        }
    }

    private void ApplyFilters(ScheduleDefinition definition) {
        foreach(var rule in ScheduleFilterList.ScheduleFilterRules) {
            // Ошибочное правило пропускаем, чтобы остальные фильтры все равно применились
            try {
                var filter = CreateFilter(definition, rule);
                if(filter != null) {
                    definition.AddFilter(filter);
                }
            } catch(System.Exception) { }
        }
    }

    private ScheduleFilter CreateFilter(ScheduleDefinition definition, ScheduleFilterRuleVM rule) {
        var filterType = rule.SelectedFilterType.FilterType;
        if(filterType == ScheduleFilterType.HasValue
            || filterType == ScheduleFilterType.HasNoValue
            || filterType == ScheduleFilterType.HasParameter) {
            // Создаем фильтр для поля по правилу имеет/не имеет значение/параметр
            return CreateHasFilter(definition, rule);
        }

        if(Repository.IsWorksetField(definition, rule.SelectedSpecField.Field.FieldId)) {
            // Создаем фильтр для поля "Рабочий набор"
            return CreateWorksetFilter(rule);
        }

        // Создаем фильтр для обычного поля
        return CreateStandardFilter(definition, rule);
    }

    /// <summary>
    /// Создает фильтр по рабочему набору. Для условий "равно" и "не равно" Revit сравнивает id набора,
    /// поэтому ищется набор с точно таким именем; если его нет, фильтр не создается.
    /// Остальные условия (содержит, начинается с и т.д.) Revit сравнивает с именем набора как со строкой,
    /// поэтому значение передается как есть
    /// </summary>
    private ScheduleFilter CreateWorksetFilter(ScheduleFilterRuleVM rule) {
        var fieldId = rule.SelectedSpecField.Field.FieldId;
        var filterType = rule.SelectedFilterType.FilterType;

        if(filterType is ScheduleFilterType.Equal or ScheduleFilterType.NotEqual) {
            var workset = Repository.FindWorksetByName(rule.FilterValue);
            return workset is null
                ? null
                : new ScheduleFilter(fieldId, filterType, workset.Id.IntegerValue);
        }

        return new ScheduleFilter(fieldId, filterType, rule.FilterValue ?? string.Empty);
    }

    private ScheduleFilter CreateStandardFilter(ScheduleDefinition definition, ScheduleFilterRuleVM rule) {
        var fieldId = rule.SelectedSpecField.Field.FieldId;
        var filterType = rule.SelectedFilterType.FilterType;
        string filterValue = rule.FilterValue;

        if(definition.CanFilterBySubstring(fieldId)) {
            return new ScheduleFilter(
                fieldId,
                filterType,
                filterValue);
        } else {
            if(int.TryParse(filterValue, out int value)) {
                return new ScheduleFilter(
                    fieldId,
                    filterType,
                    value);
            }
        }
        return null;
    }

    private ScheduleFilter CreateHasFilter(ScheduleDefinition definition, ScheduleFilterRuleVM rule) {
        var fieldId = rule.SelectedSpecField.Field.FieldId;
        var filterType = rule.SelectedFilterType.FilterType;

        if(definition.CanFilterByParameterExistence(fieldId) || definition.CanFilterByValuePresence(fieldId)) {
            return new ScheduleFilter(fieldId, filterType);
        }
        return null;
    }
}
