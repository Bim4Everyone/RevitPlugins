using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionServices;
internal class DimensionCreator {
    private readonly RevitRepository _revitRepository;

    public DimensionCreator(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public Dimension Create(Line dimensionLine, params Reference[] references) {
        dimensionLine.ThrowIfNull();
        references.ThrowIfNullOrEmpty();

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray);
    }

    public Dimension Create(Line dimensionLine, DimensionType selectedDimensionType, params Reference[] references) {
        dimensionLine.ThrowIfNull();
        selectedDimensionType.ThrowIfNull();
        references.ThrowIfNullOrEmpty();

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray, selectedDimensionType);
    }

    public Dimension Create(Line dimensionLine, ReferenceArray referenceArray, DimensionType selectedDimensionType = null) {
        dimensionLine.ThrowIfNull();
        referenceArray.ThrowIfNull();

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

    public Dimension RecreateDimension(Dimension oldDimension, DimensionTextPoints points, ReferenceArray references) {
        oldDimension.ThrowIfNull();
        points.ThrowIfNull();
        references.ThrowIfNull();

        var dimensionLine = Line.CreateBound(points.BottomLeftCorner, points.BottomRightCorner);
        var newDimension = Create(dimensionLine, references, oldDimension.DimensionType);
        newDimension.TextPosition = points.TextPositionPoint;
        _revitRepository.Document.Delete(oldDimension.Id);
        return newDimension;
    }
}
