using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;
internal class DimensionCreator {
    private readonly RevitRepository _revitRepository;

    public DimensionCreator(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public Dimension Create(Line dimensionLine, params Reference[] references) {
        if(dimensionLine == null)
            throw new ArgumentNullException(nameof(dimensionLine));
        if(references == null)
            throw new ArgumentNullException(nameof(references));

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray);
    }

    public Dimension Create(Line dimensionLine, DimensionType selectedDimensionType, params Reference[] references) {
        if(dimensionLine == null)
            throw new ArgumentNullException(nameof(dimensionLine));
        if(selectedDimensionType == null)
            throw new ArgumentNullException(nameof(selectedDimensionType));
        if(references == null)
            throw new ArgumentNullException(nameof(references));

        var refArray = new ReferenceArray();
        foreach(var reference in references) {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));
            refArray.Append(reference);
        }
        return Create(dimensionLine, refArray, selectedDimensionType);
    }

    public Dimension Create(Line dimensionLine, ReferenceArray referenceArray, DimensionType selectedDimensionType = null) {
        if(dimensionLine == null)
            throw new ArgumentNullException(nameof(dimensionLine));
        if(referenceArray == null)
            throw new ArgumentNullException(nameof(referenceArray));

        return selectedDimensionType is null
            ? _revitRepository.Document.Create.NewDimension(
                _revitRepository.Document.ActiveView,
                dimensionLine,
                referenceArray)
            : _revitRepository.Document.Create.NewDimension(
                _revitRepository.Document.ActiveView,
                dimensionLine,
                referenceArray,
                selectedDimensionType);
    }
}
