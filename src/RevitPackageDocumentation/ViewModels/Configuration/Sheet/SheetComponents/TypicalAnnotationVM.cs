using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TypicalAnnotationVM : SheetComponentVM {
    private List<AnnotationSymbolType> _annotationTypes;
    private Family _annotationFamily;
    private AnnotationSymbolType _annotationType;

    public TypicalAnnotationVM(
        SheetVM sheetVM,
        RevitRepository repository,
        ILocalizationService localizationService,
        StringParamSetService stringParamSetService)
        : base(sheetVM, repository, localizationService, stringParamSetService) {
        SelectAnnotationFamilyCommand = RelayCommand.Create(SelectAnnotationFamily);
        CreateComponentCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public ICommand SelectAnnotationFamilyCommand { get; }

    public List<AnnotationSymbolType> AnnotationTypes {
        get => _annotationTypes;
        set => RaiseAndSetIfChanged(ref _annotationTypes, value);
    }

    public Family AnnotationFamily {
        get => _annotationFamily;
        set => RaiseAndSetIfChanged(ref _annotationFamily, value);
    }

    public AnnotationSymbolType AnnotationType {
        get => _annotationType;
        set => RaiseAndSetIfChanged(ref _annotationType, value);
    }

    private void SelectAnnotationFamily() {
        AnnotationType = null;
        SetAnnotationTypes(AnnotationFamily);
    }

    public void SetAnnotationTypes(Family annotationFamily) {
        AnnotationTypes = annotationFamily
            ?.GetFamilySymbolIds()
            ?.Select(id => Repository.Document.GetElement(id) as AnnotationSymbolType)
            ?.ToList();
    }

    public override void CreateComponent() { }

    public override bool ValidateModule() {
        if(AnnotationFamily is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.AnnotationFamilyIsNull");
            return false;
        }
        if(AnnotationType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.AnnotationTypeIsNull");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }

    public override void Process() {
        var instance = Place();
        SetCustomParams(instance);
    }

    public FamilyInstance Place() {
        var position = new XYZ(
            UnitUtilsHelper.ConvertToInternalValue(-100),
            UnitUtilsHelper.ConvertToInternalValue(250),
            0);
        return Repository.Document.Create.NewFamilyInstance(position, AnnotationType, Sheet.SheetInstance);
    }
}
