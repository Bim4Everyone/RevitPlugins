using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class ScheduleViewVM : SheetComponentVM {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _viewName;
    private string _viewColumn;
    private string _viewCount;
    private ViewSchedule _referenceSpec;

    // Смещение по горизонтали в футах, для размещаемых на листе спецификациях требуемое, чтобы они попали на лист
    private readonly double _specViewportRightOffset = UnitUtilsHelper.ConvertToInternalValue(0.77);

    // Смещение по вертикали в футах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _specViewportTopOffset = UnitUtilsHelper.ConvertToInternalValue(12);

    public ScheduleViewVM(SheetVM sheetVM, RevitRepository revitRepository, ILocalizationService localizationService) : base(sheetVM) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }


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
