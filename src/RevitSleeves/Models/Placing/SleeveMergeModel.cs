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

    public double GetDiameter() {
        return _sleeves[0].Diameter;
    }

    public XYZ GetOrientation() {
        return _sleeves[0].GetOrientation();
    }

    public (XYZ Start, XYZ End) GetEndPoints() {
        var orientation = GetOrientation();
        var startPoint = _sleeves[0].Location;
        var orderedSleeves = _sleeves.OrderBy(s => orientation.DotProduct(s.Location - startPoint)).ToArray();
        var first = orderedSleeves.First();
        var last = orderedSleeves.Last();
        double length = first.Length / 2 + (last.Location - first.Location).GetLength() + last.Length / 2;
        var start = first.Location - orientation * first.Length / 2;
        return (start, start + orientation * length);
    }

    public IReadOnlyCollection<SleeveModel> GetSleeves() {
        return new ReadOnlyCollection<SleeveModel>(_sleeves);
    }

    public bool CanAddSleeve(SleeveModel sleeve) {
        return _sleeves.Any(s => s.CanMerge(sleeve));
    }

    public bool TryAddSleeve(SleeveModel sleeve) {
        bool canAdd = CanAddSleeve(sleeve);
        if(canAdd) {
            _sleeves.Add(sleeve);
        }
        return canAdd;
    }
}
