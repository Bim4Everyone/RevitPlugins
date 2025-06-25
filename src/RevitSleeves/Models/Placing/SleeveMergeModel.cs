using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    public IReadOnlyCollection<SleeveModel> GetSleeves() {
        return new ReadOnlyCollection<SleeveModel>(_sleeves);
    }

    public bool CanAddSleeve(SleeveModel sleeve) {
        throw new NotImplementedException();
    }

    public bool TryAddSleeve(SleeveModel sleeve) {
        bool canAdd = CanAddSleeve(sleeve);
        if(canAdd) {
            _sleeves.Add(sleeve);
        }
        return canAdd;
    }
}
