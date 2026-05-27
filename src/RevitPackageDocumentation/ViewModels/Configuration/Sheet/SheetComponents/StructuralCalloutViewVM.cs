using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class StructuralCalloutViewVM : SheetComponentVM {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    // Ширина фрагмента
    private readonly double _calloutWidth = UnitUtilsHelper.ConvertToInternalValue(4000);

    // Высота фрагмента
    private readonly double _calloutHeight = UnitUtilsHelper.ConvertToInternalValue(3000);

    // Отступ между фрагментами в модели
    private readonly double _viewsOffset = UnitUtilsHelper.ConvertToInternalValue(5000);

    // Отступ между видовыми экранами на листе
    private readonly double _viewportOffset = UnitUtilsHelper.ConvertToInternalValue(150);

    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewPlan _viewTemplate;
    private string _viewCount;
    private string _viewportNumber;

    public StructuralCalloutViewVM(SheetVM sheetVM, RevitRepository revitRepository, ILocalizationService localizationService) : base(sheetVM) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public string ViewName {
        get => _viewName;
        set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public ViewFamilyType ViewFamilyType {
        get => _viewFamilyType;
        set => RaiseAndSetIfChanged(ref _viewFamilyType, value);
    }

    public ElementType ViewportType {
        get => _viewportType;
        set => RaiseAndSetIfChanged(ref _viewportType, value);
    }

    public ViewPlan ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
    }

    public string ViewportNumber {
        get => _viewportNumber;
        set => RaiseAndSetIfChanged(ref _viewportNumber, value);
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(string.IsNullOrEmpty(ViewName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewNameIsEmpty");
            return false;
        }
        if(ViewFamilyType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewFamilyTypeIsNull");
            return false;
        }
        if(ViewportType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewportTypeIsNull");
            return false;
        }
        if(ViewTemplate is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewTemplateIsNull");
            return false;
        }
        if(!double.TryParse(ViewCount, out double viewCountAsDouble) || viewCountAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ViewCountIsNotCorrect");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() {
        int.TryParse(ViewCount, out int viewCountAsInt);

        for(int i = 1; i <= viewCountAsInt; i++) {
            var view = Create(i);
            //Place(view, i);
        }
    }

    public View Create(int number) {
        var viewName = $"{ViewName}_{number}";
        var view = _revitRepository.GetViewByName(viewName);

        if(view != null) {
            return view;
        }

        try {
            var index = Sheet.SheetComponents.IndexOf(this);
            StructuralPlanViewVM parentView = default;

            for(int i = index; i >= 0; i--) {
                parentView = Sheet.SheetComponents[i] as StructuralPlanViewVM;

                if(parentView != null) {
                    break;
                }
            }

            if(parentView is null || parentView.ViewInstance is null) {
                return null;
            }

            parentView.ViewInstance.CropBoxActive = true;
            var cropBox = parentView.ViewInstance.CropBox;
            var parentViewMiddle = (cropBox.Max + cropBox.Min) / 2;
            parentView.ViewInstance.CropBoxActive = false;
            parentView.ViewInstance.CropBoxVisible = false;

            var calloutStart = parentViewMiddle + new XYZ(_viewsOffset * (number - 1), 0, 0);
            var calloutEnd = calloutStart + new XYZ(_calloutWidth, _calloutHeight, 0);

            view = ViewSection.CreateCallout(
                _revitRepository.Document,
                parentView.ViewInstance.Id,
                ViewFamilyType.Id,
                calloutStart,
                calloutEnd);
            view.Name = viewName;
            view.ViewTemplateId = ViewTemplate.Id;

            // Необходимо для перезагрузки габаритов видов перед их размещением, т.к. при назначении 
            // секущего диапазона, видимых категорий, шаблона вида могут изменяться габариты вида
            _revitRepository.Document.Regenerate();
        } catch(System.Exception) { }
        return view;
    }

    public void Place() { }
}
