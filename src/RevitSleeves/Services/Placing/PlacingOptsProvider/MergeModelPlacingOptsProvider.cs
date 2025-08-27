using System.Linq;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Placing.LevelFinder;
using RevitSleeves.Services.Placing.ParamsSetterFinder;
using RevitSleeves.Services.Placing.PointFinder;
using RevitSleeves.Services.Placing.RotationFinder;

namespace RevitSleeves.Services.Placing.PlacingOptsProvider;
internal class MergeModelPlacingOptsProvider : PlacingOptsProvider<SleeveMergeModel> {
    public MergeModelPlacingOptsProvider(
        IFamilySymbolFinder symbolFinder,
        ILevelFinder<SleeveMergeModel> levelFinder,
        IPointFinder<SleeveMergeModel> pointFinder,
        IRotationFinder<SleeveMergeModel> rotationFinder,
        IParamsSetterFinder<SleeveMergeModel> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }

    protected override Element[] GetDependentElements(SleeveMergeModel param) {
        return [.. param.GetSleeves().Select(s => s.GetFamilyInstance() as Element)];
    }
}
