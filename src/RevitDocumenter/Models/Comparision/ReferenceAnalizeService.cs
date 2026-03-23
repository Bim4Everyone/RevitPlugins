using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.DimensionServices;

namespace RevitDocumenter.Models.Comparision;
internal class ReferenceAnalizeService {
    private readonly RevitRepository _revitRepository;
    private readonly DimensionCreator _dimensionCreator;
    private readonly ValueGuard _guard;

    public ReferenceAnalizeService(
        RevitRepository revitRepository,
        DimensionCreator dimensionCreator,
        ValueGuard guard) {

        _revitRepository = revitRepository;
        _dimensionCreator = dimensionCreator;
        _guard = guard;
    }

    /// <summary>
    /// Получает список опорных плоскостей размера по ключевым словам в их именах
    /// </summary>
    public List<Reference> GetDimensionRefList(FamilyInstance elem, List<string> importantRefNameParts) {
        var allRefs = new List<Reference>();
        foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
            allRefs.AddRange(elem.GetReferences(referenceType));
        }
        var refs = new List<Reference>();
        foreach(var reference in allRefs) {
            if(importantRefNameParts.Contains(elem.GetReferenceName(reference))) {
                refs.Add(reference);
            }
        }
        return refs;
    }

    /// <summary>
    /// Метод по поиску ближайших опорных плоскостей из двух списков
    /// </summary>
    /// <param name="refsA">Первый список опорных плоскостей</param>
    /// <param name="refsB">Второй список опорных плоскостей</param>
    /// <param name="dimensionLine">Линия, вдоль которой нужно строить временные размеры</param>
    /// <returns>Массив из пары опорных плоскостей, которые стоят наиболее близко</returns>
    public ReferenceArray FindClosestReferencesByDimension(List<Reference> refsA, List<Reference> refsB, Line dimensionLine) {
        _guard.ThrowIfNullOrEmpty(refsA, refsB, dimensionLine);

        Reference resultRef1 = null;
        Reference resultRef2 = null;
        double minDistance = double.MaxValue;

        using(var subTransaction = new SubTransaction(_revitRepository.Document)) {
            subTransaction.Start();
            try {
                foreach(var refA in refsA) {
                    foreach(var refB in refsB) {
                        var dimension = _dimensionCreator.Create(dimensionLine, refA, refB);

                        if(dimension?.Value.HasValue == true) {
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
        return CreateReferenceArray(resultRef1, resultRef2);
    }

    private ReferenceArray CreateReferenceArray(Reference ref1, Reference ref2) {
        if(ref1 == null || ref2 == null)
            return null;

        var refArray = new ReferenceArray();
        refArray.Append(ref1);
        refArray.Append(ref2);
        return refArray;
    }


    public bool IsReferenceArrayInList(ReferenceArray refArray, List<ReferenceArray> listRefArrayForCheck) {
        if(refArray.Size != 2) {
            return false;
        }
        var checkId1 = refArray.get_Item(0).ElementId;
        var checkId2 = refArray.get_Item(1).ElementId;

        foreach(var refArrayForCheck in listRefArrayForCheck) {
            if(refArrayForCheck.Size != 2) {
                continue;
            }
            var elemId1 = refArrayForCheck.get_Item(0).ElementId;
            var elemId2 = refArrayForCheck.get_Item(1).ElementId;

            bool test1 = checkId1.Equals(elemId1) && checkId2.Equals(elemId2);
            bool test2 = checkId2.Equals(elemId1) && checkId1.Equals(elemId2);

            if(test1 || test2) {
                return true;
            }
        }
        return false;
    }
}
