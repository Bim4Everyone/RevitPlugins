using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class MergeModelPlacingOptsProvider : PlacingOptsProvider<SleeveMergeModel> {
    public MergeModelPlacingOptsProvider(
        IFamilySymbolFinder<SleeveMergeModel> symbolFinder,
        ILevelFinder<SleeveMergeModel> levelFinder,
        IPointFinder<SleeveMergeModel> pointFinder,
        IRotationFinder<SleeveMergeModel> rotationFinder,
        IParamsSetterFinder<SleeveMergeModel> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }
}
