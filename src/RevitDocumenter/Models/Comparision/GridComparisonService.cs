using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Comparision;
internal class GridComparisonService : IComparisonService {
    private readonly RevitRepository _revitRepository;
    private readonly DimensionCreator _dimensionCreator;

    public GridComparisonService(RevitRepository revitRepository, DimensionCreator dimensionCreator) {
        _revitRepository = revitRepository;
        _dimensionCreator = dimensionCreator;
    }


    public ReferenceArray Compare(IComparisonContext context) {
        var gridComparisonContext = context as GridComparisonContext ?? throw new ArgumentException("Invalid context type");

        var gridRefsByDirection = gridComparisonContext.Grids
            .Where(g => !g.IsCurved)
            .Where(g =>
                (g.Curve as Line).Direction.IsAlmostEqualTo(gridComparisonContext.Direction)
                    || (g.Curve as Line).Direction.IsAlmostEqualTo(gridComparisonContext.Direction.Negate()))
            .Select(g => new Reference(g))
            .ToList();

        var line = Line.CreateBound(XYZ.Zero, XYZ.Zero + gridComparisonContext.Direction.CrossProduct(XYZ.BasisZ));

        (Reference ref1, Reference ref2)? ff = FindClosestReferences(gridComparisonContext.RebarReferences, gridRefsByDirection, line);

        var refArray = new ReferenceArray();
        if(ff != null && ff.Value.ref1 != null) {
            refArray.Append(ff.Value.ref1);
        }
        if(ff != null && ff.Value.ref2 != null) {
            refArray.Append(ff.Value.ref2);
        }
        return refArray;
    }


    public (Reference ref1, Reference ref2)? FindClosestReferences(
        List<Reference> referencesA,
        List<Reference> referencesB,
        Line dimensionLine) {

        if(referencesA == null || referencesA.Count == 0)
            throw new ArgumentNullException(nameof(referencesA));
        if(referencesB == null || referencesB.Count == 0)
            throw new ArgumentNullException(nameof(referencesB));
        if(dimensionLine == null)
            throw new ArgumentNullException(nameof(dimensionLine));

        Reference resultRef1 = null;
        Reference resultRef2 = null;
        double minDistance = double.MaxValue;

        // Создаем временную транзакцию для создания размеров
        using(SubTransaction subTransaction = new SubTransaction(_revitRepository.Document)) {
            subTransaction.Start();

            try {
                // Создаем размер между каждой парой
                foreach(var refA in referencesA) {
                    foreach(var refB in referencesB) {
                        Dimension dimension = _dimensionCreator.Create(dimensionLine, refA, refB);

                        if(dimension != null && dimension.Value.HasValue) {
                            double distance = dimension.Value.Value;

                            if(distance < minDistance) {
                                minDistance = distance;
                                resultRef1 = refA;
                                resultRef2 = refB;
                            }
                        }
                    }
                }
                subTransaction.RollBack();
            } catch(Exception) {
                subTransaction.RollBack();
            }
        }

        if(resultRef1 != null && resultRef2 != null) {
            return (resultRef1, resultRef2);
        }
        return null;
    }
}
