using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class PipeFloorPlacingOptsProvider : PlacingOptsProvider<ClashModel<Pipe, Floor>> {
    public PipeFloorPlacingOptsProvider(
        IFamilySymbolFinder<ClashModel<Pipe, Floor>> symbolFinder,
        ILevelFinder<ClashModel<Pipe, Floor>> levelFinder,
        IPointFinder<ClashModel<Pipe, Floor>> pointFinder,
        IRotationFinder<ClashModel<Pipe, Floor>> rotationFinder,
        IParamsSetterFinder<ClashModel<Pipe, Floor>> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }
}
