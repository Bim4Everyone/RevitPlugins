using System;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Navigator;

namespace RevitSleeves.Models.Placing;
internal class SleeveModel {
    private readonly FamilyInstance _familyInstance;

    public SleeveModel(FamilyInstance familyInstance) {
        _familyInstance = familyInstance ?? throw new ArgumentNullException(nameof(familyInstance));
    }


    public SleeveStatus Status { get; set; }


    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }
}
