using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class PipeWallPlacingOptsProvider : PlacingOptsProvider<ClashModel<Pipe, Wall>> {
    public PipeWallPlacingOptsProvider(
        IFamilySymbolFinder<ClashModel<Pipe, Wall>> symbolFinder,
        ILevelFinder<ClashModel<Pipe, Wall>> levelFinder,
        IPointFinder<ClashModel<Pipe, Wall>> pointFinder,
        IRotationFinder<ClashModel<Pipe, Wall>> rotationFinder,
        IParamsSetterFinder<ClashModel<Pipe, Wall>> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }
}
