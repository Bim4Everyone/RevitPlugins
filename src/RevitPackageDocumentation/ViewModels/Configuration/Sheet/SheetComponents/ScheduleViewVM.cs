using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.ScheduleFilters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class ScheduleViewVM : SheetComponentVM {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _viewName;
    private string _viewColumn;
    private string _viewCount;
    private ViewSchedule _referenceSpec;
    private ScheduleFilterListVM _scheduleFilterList;

    // Смещение по горизонтали в футах, для размещаемых на листе спецификациях требуемое, чтобы они попали на лист
    private readonly double _specViewportRightOffset = UnitUtilsHelper.ConvertToInternalValue(0.77);

    // Смещение по вертикали в футах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _specViewportTopOffset = UnitUtilsHelper.ConvertToInternalValue(12);

    public ScheduleViewVM(SheetVM sheetVM, RevitRepository revitRepository, ILocalizationService localizationService) : base(sheetVM) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
        SelectReferenceSpecCommand = RelayCommand.Create(SelectReferenceSpec);

        ScheduleFilterList = new ScheduleFilterListVM(this);
    }

    public ICommand SelectReferenceSpecCommand { get; set; }

    public ViewSchedule ReferenceSpec {
        get => _referenceSpec;
        set => RaiseAndSetIfChanged(ref _referenceSpec, value);
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
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ReferenceViewNameIsEmpty");
            return false;
        }
        if(string.IsNullOrEmpty(ViewName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(!int.TryParse(ViewColumn, out int viewColumnAsInt) || viewColumnAsInt < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewColumnIsNotCorrect");
            return false;
        }
        if(!int.TryParse(ViewCount, out int viewCountAsInt) || viewCountAsInt < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    private void SelectReferenceSpec() {
        ScheduleFilterList.SetSchedule(ReferenceSpec);
    }

    public override void Process() {
        var view = Create();
        Place(view);
    }

    public ViewSchedule Create() {
        var view = _revitRepository.GetSpecByName(ViewName);
        if(view != null) {
            return view;
        }

        if(ReferenceSpec is null || !ReferenceSpec.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return null;
        }

        try {
            var scheduleId = ReferenceSpec.Duplicate(ViewDuplicateOption.Duplicate);
            view = _revitRepository.Document.GetElement(scheduleId) as ViewSchedule;
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


    public void Place(ViewSchedule view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance is null
            || view is null
            || Viewport.CanAddViewToSheet(_revitRepository.Document, sheetInstance.Id, view.Id)) {
            return;
        }

        // Получение габаритов рамки листа
        if(_revitRepository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
            return;
        }
        var titleBlockBB = titleBlock.get_BoundingBox(sheetInstance);
        double titleBlockWidth = titleBlockBB.Max.X - titleBlockBB.Min.X;
        double titleBlockHeight = titleBlockBB.Max.Y - titleBlockBB.Min.Y;


        var scheduleSheetInstance = ScheduleSheetInstance.Create(
            _revitRepository.Document, sheetInstance.Id, view.Id, XYZ.Zero);

        // Точка вставки у спеки в верхнем левом углу спеки
        var viewportBB = scheduleSheetInstance.get_BoundingBox(sheetInstance);
        double viewportWidth = viewportBB.Max.X - viewportBB.Min.X;

        var newCenter = new XYZ(
            -viewportWidth - _specViewportRightOffset,
            titleBlockHeight - _specViewportTopOffset,
            0);
        scheduleSheetInstance.Point = newCenter;
    }
}
