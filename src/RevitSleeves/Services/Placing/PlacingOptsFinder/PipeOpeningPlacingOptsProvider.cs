using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
internal class PipeOpeningPlacingOptsProvider : PlacingOptsProvider<ClashModel<Pipe, FamilyInstance>> {
    public PipeOpeningPlacingOptsProvider(
        IFamilySymbolFinder<ClashModel<Pipe, FamilyInstance>> symbolFinder,
        ILevelFinder<ClashModel<Pipe, FamilyInstance>> levelFinder,
        IPointFinder<ClashModel<Pipe, FamilyInstance>> pointFinder,
        IRotationFinder<ClashModel<Pipe, FamilyInstance>> rotationFinder,
        IParamsSetterFinder<ClashModel<Pipe, FamilyInstance>> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }
}
