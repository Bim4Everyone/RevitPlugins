using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal class TypicalAnnotationVM : SheetComponentVM {
    private readonly RevitRepository _revitRepository;

    private List<AnnotationSymbolType> _annotationTypes;
    private Family _annotationFamily;
    private AnnotationSymbolType _annotationType;

    public TypicalAnnotationVM(RevitRepository revitRepository) {
        _revitRepository = revitRepository;

        SelectAnnotationFamilyCommand = RelayCommand.Create(SelectAnnotationFamily);
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
            ?.Select(id => _revitRepository.Document.GetElement(id) as AnnotationSymbolType)
            ?.ToList();
    }

    public override void ValidateModule() { }
    public override void Process() { }

    public void Place() { }
}
