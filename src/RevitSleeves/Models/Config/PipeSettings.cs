using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Config;
internal class PipeSettings : MepCategorySettings {
    public PipeSettings() : base() {
        if(Offsets is null || Offsets.Length == 0) {
            Offsets = [
                new Offset() {
                    OffsetType = OffsetType.FromSleeveAxisToMepAxis,
                    Value = 5
                },
                new Offset() {
                    OffsetType = OffsetType.FromSleeveEndToTopFloorFace,
                    Value = 50
                }];
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
