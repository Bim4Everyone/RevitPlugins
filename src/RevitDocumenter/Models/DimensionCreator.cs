using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;
internal class DimensionCreator {
    private readonly RevitRepository _revitRepository;
    private readonly ArgumentValidator _argumentValidator;

    public DimensionCreator(RevitRepository revitRepository, ArgumentValidator argumentValidator) {
        _revitRepository = revitRepository;
        _argumentValidator = argumentValidator;
    }

    public Dimension Create(Line dimensionLine, params Reference[] references) {
        _argumentValidator.Validate(dimensionLine, references);

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray);
    }

    public Dimension Create(Line dimensionLine, DimensionType selectedDimensionType, params Reference[] references) {
        _argumentValidator.Validate(dimensionLine, selectedDimensionType, references);

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray, selectedDimensionType);
    }

    public Dimension Create(Line dimensionLine, ReferenceArray referenceArray, DimensionType selectedDimensionType = null) {
        _argumentValidator.Validate(dimensionLine, referenceArray);

        using var subTransaction = new SubTransaction(_revitRepository.Document);
        subTransaction.Start();

        var dimension = selectedDimensionType is null
            ? _revitRepository.Document.Create.NewDimension(
                _revitRepository.Document.ActiveView,
                dimensionLine,
                referenceArray)
            : _revitRepository.Document.Create.NewDimension(
                _revitRepository.Document.ActiveView,
                dimensionLine,
                referenceArray,
                selectedDimensionType);
        subTransaction.Commit();
        return dimension;
    }
}
