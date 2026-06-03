using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.ScheduleFilters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class ScheduleViewVM : SheetComponentVM {
    private string _viewNameFormula = string.Empty;
    private string _viewName;
    private string _viewColumn;
    private string _viewCount;
    private ViewSchedule _referenceSpec;
    private ScheduleFilterListVM _scheduleFilterList;

    // Смещение по горизонтали в футах, для размещаемых на листе спецификациях требуемое, чтобы они попали на лист
    private readonly double _specViewportRightOffset = UnitUtilsHelper.ConvertToInternalValue(0.77);

    // Смещение по вертикали в футах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _specViewportTopOffset = UnitUtilsHelper.ConvertToInternalValue(12);

    public ScheduleViewVM(
        SheetVM sheetVM,
        RevitRepository repository,
        ILocalizationService localizationService,
        StringParamSetService stringParamSetService)
        : base(sheetVM, repository, localizationService, stringParamSetService) {
        ScheduleFilterList = new ScheduleFilterListVM(this);

        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
        SelectReferenceSpecCommand = RelayCommand.Create(SelectReferenceSpec);
    }

    public ICommand SelectReferenceSpecCommand { get; set; }

    public ViewSchedule ReferenceSpec {
        get => _referenceSpec;
        set => RaiseAndSetIfChanged(ref _referenceSpec, value);
    }

    public string ViewNameFormula {
        get => _viewNameFormula;
        set => RaiseAndSetIfChanged(ref _viewNameFormula, value);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public string ViewColumn {
        get => _viewColumn;
        set => RaiseAndSetIfChanged(ref _viewColumn, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public ScheduleFilterListVM ScheduleFilterList {
        get => _scheduleFilterList;
        set => RaiseAndSetIfChanged(ref _scheduleFilterList, value);
    }


    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(ReferenceSpec is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ReferenceViewNameIsEmpty");
            return false;
        }
        if(string.IsNullOrEmpty(ViewNameFormula)) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(!int.TryParse(ViewColumn, out int viewColumnAsInt) || viewColumnAsInt < 1) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewColumnIsNotCorrect");
            return false;
        }
        if(!int.TryParse(ViewCount, out int viewCountAsInt) || viewCountAsInt < 1) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
            return false;
        }
        foreach(var rule in ScheduleFilterList.ScheduleFilterRules) {
            if(rule.SelectedSpecField is null || rule.SelectedFilterType is null) {
                ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.ScheduleFiltersIsNotCorrect");
                return false;
            }
        }
        foreach(var param in CustomParamsList.Params) {
            if(string.IsNullOrEmpty(param.ParamName)) {
                ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.CustomParamsIsNotCorrect");
                return false;
            }
        }

        ModuleErrors = string.Empty;
        return true;
    }

    private void SelectReferenceSpec() {
        ScheduleFilterList.SetSchedule(ReferenceSpec);
    }

    public override void Process() {
        var view = Create();
        var instance = Place(view);
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
            for(int i = definition.GetFilters().Count - 1; i >= 0; i--) {
                definition.RemoveFilter(i);
            }
            // Добавляем указанные пользователем фильтры
            foreach(var rule in ScheduleFilterList.ScheduleFilterRules) {
                ScheduleFilter filter = default;

                var fieldId = rule.SelectedSpecField.Field.FieldId;
                if(definition.CanFilterBySubstring(fieldId)) {
                    filter = new ScheduleFilter(
                        fieldId,
                        rule.SelectedFilterType.FilterType,
                        rule.FilterValue);
                } else {
                    if(int.TryParse(rule.FilterValue, out int value)) {
                        filter = new ScheduleFilter(
                            fieldId,
                            rule.SelectedFilterType.FilterType,
                            value);
                    }
                }
                definition.AddFilter(filter);
            }
        } catch(System.Exception) { }
        return view;
    }


    public ScheduleSheetInstance Place(ViewSchedule view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance is null
            || view is null
            || Viewport.CanAddViewToSheet(Repository.Document, sheetInstance.Id, view.Id)) {
            return null;
        }

        // Получение габаритов рамки листа
        if(Repository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
            return null;
        }
        var titleBlockBB = titleBlock.get_BoundingBox(sheetInstance);
        double titleBlockWidth = titleBlockBB.Max.X - titleBlockBB.Min.X;
        double titleBlockHeight = titleBlockBB.Max.Y - titleBlockBB.Min.Y;


        var scheduleSheetInstance = ScheduleSheetInstance.Create(
            Repository.Document, sheetInstance.Id, view.Id, XYZ.Zero);

        // Точка вставки у спеки в верхнем левом углу спеки
        var viewportBB = scheduleSheetInstance.get_BoundingBox(sheetInstance);
        double viewportWidth = viewportBB.Max.X - viewportBB.Min.X;

        var newCenter = new XYZ(
            -viewportWidth - _specViewportRightOffset,
            titleBlockHeight - _specViewportTopOffset,
            0);
        scheduleSheetInstance.Point = newCenter;
        return scheduleSheetInstance;
    }
}
