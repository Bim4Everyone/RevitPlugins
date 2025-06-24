using System;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class MergeModelParamsSetter : IParamsSetter<SleeveMergeModel> {
    private readonly SleeveMergeModel _sleeveModel;

    public MergeModelParamsSetter(SleeveMergeModel sleeveModel) {
        _sleeveModel = sleeveModel ?? throw new ArgumentNullException(nameof(sleeveModel));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        throw new NotImplementedException();
    }
}
