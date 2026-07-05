using System.Collections.Generic;
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

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TypicalAnnotationVM : SheetComponentVM {
    private List<AnnotationSymbolType> _annotationTypes;
    private Family _annotationFamily;
    private AnnotationSymbolType _annotationType;

    private FiltrationComboBoxFilterListVM _annotationFamilyFilter;
    private FiltrationComboBoxFilterListVM _annotationTypeFilter;

    public TypicalAnnotationVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {
        ValidateAllProperties();

        SelectAnnotationFamilyCommand = RelayCommand.Create(SelectAnnotationFamily);
        CreateComponentCommand = RelayCommand.Create(CreateComponent, CanCreateComponent);
    }

    public ICommand SelectAnnotationFamilyCommand { get; }

    [Required(ErrorMessage = "Validation.AnnotationFamilyIsNull")]
    public Family AnnotationFamily {
        get => _annotationFamily;
        set => RaiseAndSetIfChanged(ref _annotationFamily, value);
    }

    public FiltrationComboBoxFilterListVM AnnotationFamilyFilter {
        get => _annotationFamilyFilter;
        set => RaiseAndSetIfChanged(ref _annotationFamilyFilter, value);
    }

    public List<AnnotationSymbolType> AnnotationTypes {
        get => _annotationTypes;
        set => RaiseAndSetIfChanged(ref _annotationTypes, value);
    }

    [Required(ErrorMessage = "Validation.AnnotationTypeIsNull")]
    public AnnotationSymbolType AnnotationType {
        get => _annotationType;
        set => RaiseAndSetIfChanged(ref _annotationType, value);
    }

    public FiltrationComboBoxFilterListVM AnnotationTypeFilter {
        get => _annotationTypeFilter;
        set => RaiseAndSetIfChanged(ref _annotationTypeFilter, value);
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
