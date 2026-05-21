using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class SectionViewVM : SheetComponentVM {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _viewName;
    private ViewFamilyType _viewFamilyType;
    private ElementType _viewportType;
    private ViewSection _viewTemplate;
    private string _viewCount;

    // Смещение по горизонтали в дюймах слева, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameLeftOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Смещение по горизонтали в дюймах справа, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameRightOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Смещение по вертикали в дюймах сверху, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    // Смещение по вертикали в дюймах снизу, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameBottomOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    // Отступ между видовыми экранами в дюймах, для корректного взаимного размещения 
    private readonly double _viewportOffset = UnitUtilsHelper.ConvertToInternalValue(10);

    public SectionViewVM(SheetVM sheetVM, RevitRepository revitRepository, ILocalizationService localizationService) : base(sheetVM) {
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

    public ViewSection ViewTemplate {
        get => _viewTemplate;
        set => RaiseAndSetIfChanged(ref _viewTemplate, value);
    }

    public string ViewCount {
        get => _viewCount;
        set => RaiseAndSetIfChanged(ref _viewCount, value);
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
        var view = Create();
        Place(view);
    }

    public ViewSection Create() {
        var view = _revitRepository.GetViewByName(ViewName) as ViewSection;

        if(view != null) {
            return view;
        }

        try {
            if(Sheet.SheetSet.Params.FirstOrDefault(p => p.ParamName == "Опалубка") is not SelectElemParamVM fromworkParam) {
                return null;
            }

            var selectedElem = fromworkParam.SelectedElem;
            var bbox = selectedElem.get_BoundingBox(null);

            XYZ center = (bbox.Min + bbox.Max) / 2;
            double widthX = bbox.Max.X - bbox.Min.X;   // размер по X
            double widthY = bbox.Max.Y - bbox.Min.Y;   // размер по Y
            double height = bbox.Max.Z - bbox.Min.Z;   // размер по Z

            // Ориентируем взгляд вдоль оси X (вправо вдоль Y, вверх вдоль Z)
            var t = Transform.Identity;
            t.Origin = center;
            t.BasisX = XYZ.BasisY;
            t.BasisY = XYZ.BasisZ;
            t.BasisZ = XYZ.BasisX;

            double offset = 1;


            double halfDepth = UnitUtilsHelper.ConvertToInternalValue(1000);

            var sectionBox = new BoundingBoxXYZ {
                Transform = t,
                Min = new XYZ(-halfDepth, -offset, -widthX / 2 - offset),
                Max = new XYZ(halfDepth, height + offset, widthX / 2 + offset)
            };

            view = ViewSection.CreateSection(_revitRepository.Document, ViewFamilyType.Id, sectionBox);
            view.Name = ViewName;
            view.ViewTemplateId = ViewTemplate.Id;

            // Необходимо для перезагрузки габаритов видов перед их размещением, т.к. при назначении 
            // секущего диапазона, видимых категорий, шаблона вида могут изменяться габариты вида
            _revitRepository.Document.Regenerate();

        } catch(System.Exception) { }
        return view;
    }

    public void Place(ViewSection view) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance != null
            && view != null
            && Viewport.CanAddViewToSheet(_revitRepository.Document, sheetInstance.Id, view.Id)) {

            // Получение габаритов рамки листа
            if(_revitRepository.GetTitleBlocks(sheetInstance) is not FamilyInstance titleBlock) {
                return;
            }
            var boundingBoxXYZ = titleBlock.get_BoundingBox(sheetInstance);
            double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
            double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

            double titleBlockMaxX = boundingBoxXYZ.Max.X;
            double titleBlockMaxY = boundingBoxXYZ.Max.Y;

            // Получение габаритов видового экрана
            var viewPort = Viewport.Create(_revitRepository.Document, sheetInstance.Id, view.Id, new XYZ(0, 0, 0));
            viewPort.ChangeTypeId(ViewportType.Id);

            var viewportCenter = viewPort.GetBoxCenter();
            var viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            var correctPosition = new XYZ(
                titleBlockMaxX - _titleBlockFrameRightOffset - viewportHalfWidth,
                titleBlockMaxY - _titleBlockFrameTopOffset - viewportHalfHeight,
                0);

            viewPort.SetBoxCenter(correctPosition);

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
#endif
        }
    }
}
