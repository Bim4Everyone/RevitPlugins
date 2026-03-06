using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;
internal class DimensionCreator {
    private readonly RevitRepository _revitRepository;
    private readonly Guard _guard;

    public DimensionCreator(RevitRepository revitRepository, Guard guard) {
        _revitRepository = revitRepository;
        _guard = guard;
    }

    public Dimension Create(Line dimensionLine, params Reference[] references) {
        _guard.ThrowIfNull(dimensionLine, references);

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray);
    }

    public Dimension Create(Line dimensionLine, DimensionType selectedDimensionType, params Reference[] references) {
        _guard.ThrowIfNull(dimensionLine, selectedDimensionType, references);

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray, selectedDimensionType);
    }

    public Dimension Create(Line dimensionLine, ReferenceArray referenceArray, DimensionType selectedDimensionType = null) {
        _guard.ThrowIfNull(dimensionLine, referenceArray);

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
