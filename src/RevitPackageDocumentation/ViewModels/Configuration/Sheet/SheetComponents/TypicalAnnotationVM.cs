using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TypicalAnnotationVM : SheetComponentVM {
    private FamilySymbol _annotationType;
    private FiltrationComboBoxFilterListVM _annotationTypeFilter;

    public TypicalAnnotationVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        ValidateAllProperties();

        CreateComponentCommand = RelayCommand.Create(CreateComponent, CanCreateComponent);
    }

    [Required(ErrorMessage = "Validation.AnnotationTypeIsNull")]
    public FamilySymbol AnnotationType {
        get => _annotationType;
        set => RaiseAndSetIfChanged(ref _annotationType, value);
    }

    public FiltrationComboBoxFilterListVM AnnotationTypeFilter {
        get => _annotationTypeFilter;
        set => RaiseAndSetIfChanged(ref _annotationTypeFilter, value);
    }

    public override void Process(bool processDependent = false) {
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
