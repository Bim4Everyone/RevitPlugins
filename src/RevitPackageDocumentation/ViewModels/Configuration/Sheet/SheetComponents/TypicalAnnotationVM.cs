using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        SelectAnnotationFamilyCommand = RelayCommand.Create(SelectAnnotationFamily);
        CreateComponentCommand = RelayCommand.Create(CreateComponent, Validate);
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

    public FiltrationComboBoxFilterListVM AnnotationFamilyFilter {
        get => _annotationFamilyFilter;
        set => RaiseAndSetIfChanged(ref _annotationFamilyFilter, value);
    }

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

    public override bool Validate() {
        if(AnnotationFamily is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.AnnotationFamilyIsNull");
            return false;
        }
        if(AnnotationType is null) {
            ModuleErrors = LocalizationService.GetLocalizedString("MainWindow.AnnotationTypeIsNull");
            return false;
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
