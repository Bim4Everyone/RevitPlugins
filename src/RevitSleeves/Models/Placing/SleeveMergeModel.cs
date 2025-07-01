using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;


namespace RevitSleeves.Models.Placing;
internal class SleeveMergeModel {
    private readonly List<SleeveModel> _sleeves;

    public SleeveMergeModel(SleeveModel sleeve) {
        if(sleeve is null) { throw new ArgumentNullException(nameof(sleeve)); }

        _sleeves = [sleeve];
        Document = sleeve.GetFamilyInstance().Document;
    }


    public Document Document { get; }

    /// <summary>
    /// Количество гильз
    /// </summary>
    public int Count => _sleeves.Count;

    public IReadOnlyCollection<SleeveModel> GetSleeves() {
        return new ReadOnlyCollection<SleeveModel>(_sleeves);
    }

    public bool CanAddSleeve(SleeveModel sleeve) {
        return _sleeves.Any(s => CanMerge(s, sleeve));
    }

    public bool TryAddSleeve(SleeveModel sleeve) {
        bool canAdd = CanAddSleeve(sleeve);
        if(canAdd) {
            _sleeves.Add(sleeve);
        }
        return canAdd;
    }

    private bool CanMerge(SleeveModel first, SleeveModel second) {
        if(first.Equals(second)) { return false; }
        double distanceTolerance = Document.Application.ShortCurveTolerance;
        if(Math.Abs(first.Diameter - second.Diameter) > distanceTolerance) {
            return false;
        }
        double angleTolerance = Document.Application.AngleTolerance;
        var locationDiff = first.Location - second.Location;
        return locationDiff.GetLength() <= (first.Length / 2 + second.Length / 2 + distanceTolerance * 2)
            && first.GetOrientation().AngleTo(locationDiff) <= angleTolerance;
    }
}
