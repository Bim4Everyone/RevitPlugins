using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitDocumenter.Models.Comparision;
internal class GridComparisonService : IComparisonService {
    private readonly RevitRepository _revitRepository;
    private readonly DimensionCreator _dimensionCreator;
    private readonly ArgumentValidator _argumentValidator;

    public GridComparisonService(
        RevitRepository revitRepository,
        DimensionCreator dimensionCreator,
        ArgumentValidator argumentValidator) {

        _revitRepository = revitRepository;
        _dimensionCreator = dimensionCreator;
        _argumentValidator = argumentValidator;
    }


    public ReferenceArray Compare(IComparisonContext context) {
        var gridComparisonContext = context as GridComparisonContext ?? throw new ArgumentException("Invalid context type");

        var gridRefsByDirection = GetGridReferencesByDirection(gridComparisonContext.Grids, gridComparisonContext.Direction);

        var line = Line.CreateBound(XYZ.Zero, XYZ.Zero + gridComparisonContext.Direction.CrossProduct(XYZ.BasisZ));

        return FindClosestReferences(gridComparisonContext.RebarReferences, gridRefsByDirection, line);
    }

    private List<Reference> GetGridReferencesByDirection(List<Grid> grids, XYZ direction) {
        return grids
            .Where(g => !g.IsCurved)
            .Where(g => IsGridLineParallelToDirection(g, direction))
            .Select(g => new Reference(g))
            .ToList();
    }

    private bool IsGridLineParallelToDirection(Grid grid, XYZ direction) {
        if(grid.Curve is Line line)
            return line.Direction.IsAlmostEqualTo(direction)
               || line.Direction.IsAlmostEqualTo(direction.Negate());
        return false;
    }

    /// <summary>
    /// Метод по поиску ближайших опорных плоскостей из двух списков
    /// </summary>
    /// <param name="referencesA">Первый список опорных плоскостей</param>
    /// <param name="referencesB">Второй список опорных плоскостей</param>
    /// <param name="dimensionLine">Линия, вдоль которой нужно строить временные размеры</param>
    /// <returns>Массив из пары опорных плоскостей, которые стоят наиболее близко</returns>
    public ReferenceArray FindClosestReferences(
        List<Reference> referencesA,
        List<Reference> referencesB,
        Line dimensionLine) {

        _argumentValidator.Validate(referencesA, referencesB, dimensionLine);

        Reference resultRef1 = null;
        Reference resultRef2 = null;
        double minDistance = double.MaxValue;

        // Создаем временную транзакцию для создания размеров
        using(var subTransaction = new SubTransaction(_revitRepository.Document)) {
            subTransaction.Start();
            try {
                // Создаем размер между каждой парой
                foreach(var refA in referencesA) {
                    foreach(var refB in referencesB) {
                        var dimension = _dimensionCreator.Create(dimensionLine, refA, refB);

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

        ReferenceArray refArray = null;
        if(resultRef1 != null && resultRef2 != null) {
            refArray = new ReferenceArray();
            refArray.Append(resultRef1);
            refArray.Append(resultRef2);
        }
        return refArray;
    }
}
