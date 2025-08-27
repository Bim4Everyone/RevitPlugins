using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Config;
internal class PipeSettings : MepCategorySettings {
    public PipeSettings() : base() {
        if(Offsets is null || Offsets.Length == 0) {
            Offsets = [
                 new Offset(){ OffsetType = OffsetType.FromSleeveAxisToMepAxis, Value = 5},
                 new Offset(){ OffsetType = OffsetType.FromSleeveEndToTopFloorFace, Value = 50},
            ];
        }
        if(DiameterRanges is null || DiameterRanges.Length == 0) {
            DiameterRanges = [
                new DiameterRange() {
                    StartMepSize = 0,
                    EndMepSize = 25,
                    SleeveDiameter = 50,
                    SleeveThickness = 4,
                },
                new DiameterRange() {
                    StartMepSize = 25,
                    EndMepSize = 50,
                    SleeveDiameter = 100,
                    SleeveThickness = 4
                },
                new DiameterRange() {
                    StartMepSize = 50,
                    EndMepSize = 100,
                    SleeveDiameter = 150,
                    SleeveThickness = 4
                },
                new DiameterRange() {
                    StartMepSize = 100,
                    EndMepSize = 150,
                    SleeveDiameter = 200,
                    SleeveThickness = 4
                },
                new DiameterRange() {
                    StartMepSize = 150,
                    EndMepSize = 200,
                    SleeveDiameter = 250,
                    SleeveThickness = 4
                }];
        }
    }

    public override BuiltInCategory Category => BuiltInCategory.OST_PipeCurves;
}
