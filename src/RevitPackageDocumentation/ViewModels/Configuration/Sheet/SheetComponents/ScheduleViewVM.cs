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
internal class ScheduleViewVM : SheetComponentVM {
    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private string _viewColumn;
    private string _viewCount;
    private ViewSchedule _referenceSpec;
    private ScheduleFilterListVM _scheduleFilterList;
    private ScheduleSheetInstance _viewportInstance;

    private FiltrationComboBoxFilterListVM _referenceSpecFilter;

    // Смещение по горизонтали в футах, для размещаемых на листе спецификациях требуемое, чтобы они попали на лист
    private readonly double _specViewportRightOffset = UnitUtilsHelper.ConvertToInternalValue(0.77);

    // Смещение по вертикали в футах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _specViewportTopOffset = UnitUtilsHelper.ConvertToInternalValue(12);

    // Смещения по вертикали в футах, для размещаемых спецификаций
    // требуемое, для их корректного взаимного размещения на листе (в случае наличия маленькой шапки спецификации)
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

    public ICommand SelectReferenceSpecCommand { get; set; }

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

    [Required(ErrorMessage = "Validation.ReferenceSpecIsNull")]
    public ViewSchedule ReferenceSpec {
        get => _referenceSpec;
        set => RaiseAndSetIfChanged(ref _referenceSpec, value);
    }

    public FiltrationComboBoxFilterListVM ReferenceSpecFilter {
        get => _referenceSpecFilter;
        set => RaiseAndSetIfChanged(ref _referenceSpecFilter, value);
    }

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

    public ScheduleSheetInstance ViewportInstance {
        get => _viewportInstance;
        set => RaiseAndSetIfChanged(ref _viewportInstance, value);
    }

    private void SelectReferenceSpec() {
        ScheduleFilterList.SetSchedule(ReferenceSpec);
    }

    public override void Process(bool processDependent = false) {
        var view = Create();
        ViewportInstance = Place(view);
        SetCustomParams(view);
    }

    public ViewSchedule Create() {
        var view = Repository.GetSpecByName(ViewName);
        if(view != null) {
            return view;
        }

        if(ReferenceSpec is null || !ReferenceSpec.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return null;
        }

        try {
            var scheduleId = ReferenceSpec.Duplicate(ViewDuplicateOption.Duplicate);
            view = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(view != null) {
                view.Name = ViewName;
            }

            var definition = view.Definition;

            // Удаляем каждый фильтр
            ClearAllFilters(definition);

            // Добавляем указанные пользователем фильтры
            ApplyFilters(definition);
        } catch(System.Exception) { }
        return view;
    }


    public ScheduleSheetInstance Place(ViewSchedule view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance is null || view is null) {
            return null;
        }

        // Если видовой экран спецификации уже есть не листе, то не размещаем повторно
        if(Repository.GetScheduleSheetInstances(sheetInstance)
            .FirstOrDefault(s => s.ScheduleId == view.Id) is ScheduleSheetInstance scheduleSheetInstance) {
            return scheduleSheetInstance;
        }

        // Получение габаритов рамки листа
        if(Repository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
            return null;
        }
        var titleBlockBB = titleBlock.get_BoundingBox(sheetInstance);
        double titleBlockWidth = titleBlockBB.Max.X - titleBlockBB.Min.X;
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
            var filter = CreateFilter(definition, rule);
            if(filter != null) {
                definition.AddFilter(filter);
            }
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

        // Пытаемся создать фильтр, где поле является параметром рабочего набора
        var worksetFilter = CreateWorksetFilter(definition, rule);

        if(worksetFilter is null) {
            // Создаем фильтр для обычного поля
            return CreateStandardFilter(definition, rule);
        } else {
            return worksetFilter;
        }
    }

    private ScheduleFilter CreateWorksetFilter(ScheduleDefinition definition, ScheduleFilterRuleVM rule) {
        var paramId = definition.GetField(rule.SelectedSpecField.Field.FieldId).ParameterId;

        if(!AreIdsEqual(paramId, Repository.WorksetParamId)) {
            return null;
        }

        // Получаем первый рабочий набор, который содержит нужную строку в имени
        var workset = new FilteredWorksetCollector(Repository.Document)
            .OfKind(WorksetKind.UserWorkset)
            .OrderBy(w => w.Name)
            .FirstOrDefault(w => w.Name.Contains(rule.FilterValue));

        if(workset is null) {
            return null;
        }

        return new ScheduleFilter(
            rule.SelectedSpecField.Field.FieldId,
            rule.SelectedFilterType.FilterType,
            workset.Id.IntegerValue);
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

    private bool AreIdsEqual(ElementId id1, ElementId id2) {
#if REVIT_2024_OR_GREATER
    return id1.Value == id2.Value;
#else
        return id1.IntegerValue == id2.IntegerValue;
#endif
    }
}
