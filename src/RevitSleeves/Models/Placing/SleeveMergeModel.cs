using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitSleeves.Models.Placing;
internal class SleeveMergeModel {
    private readonly List<SleeveModel> _sleeves;


    public SleeveMergeModel() {
        _sleeves = [];
    }


    public IReadOnlyCollection<SleeveModel> GetSleeves() {
        return new ReadOnlyCollection<SleeveModel>(_sleeves);
    }

    public bool CanAddSleeve(SleeveModel sleeve) {
        throw new NotImplementedException();
    }

    public void AddSleeve(SleeveModel sleeve) {
        throw new NotImplementedException();
    }
}
