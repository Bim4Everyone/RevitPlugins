using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Config;
internal class PipeSettings : MepCategorySettings {
    public PipeSettings() : base() {
        if(Offsets is null || Offsets.Count == 0) {
            Offsets = new System.Collections.Generic.Dictionary<OffsetType, double>() {
                { OffsetType.FromSleeveAxisToMepAxis, 5},
                { OffsetType.FromSleeveEndToTopFloorFace, 50 }
            };
        }
        if(DiameterRanges is null || DiameterRanges.Length == 0) {
            DiameterRanges = [
                new DiameterRange() {
                    StartMepSize = 0,
                    EndMepSize = 25,
                    SleeveDiameter = 50
                },
                new DiameterRange() {
                    StartMepSize = 26,
                    EndMepSize = 50,
                    SleeveDiameter = 100
                }];
        }
    }

    public override BuiltInCategory Category => BuiltInCategory.OST_PipeCurves;
}
