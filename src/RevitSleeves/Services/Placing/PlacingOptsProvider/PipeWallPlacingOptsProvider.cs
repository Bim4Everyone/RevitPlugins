using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Placing.LevelFinder;
using RevitSleeves.Services.Placing.ParamsSetterFinder;
using RevitSleeves.Services.Placing.PointFinder;
using RevitSleeves.Services.Placing.RotationFinder;

namespace RevitSleeves.Services.Placing.PlacingOptsProvider;
internal class PipeWallPlacingOptsProvider : PlacingOptsProvider<ClashModel<Pipe, Wall>> {
    public PipeWallPlacingOptsProvider(
        IFamilySymbolFinder symbolFinder,
        ILevelFinder<ClashModel<Pipe, Wall>> levelFinder,
        IPointFinder<ClashModel<Pipe, Wall>> pointFinder,
        IRotationFinder<ClashModel<Pipe, Wall>> rotationFinder,
        IParamsSetterFinder<ClashModel<Pipe, Wall>> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }

    protected override Element[] GetDependentElements(ClashModel<Pipe, Wall> param) {
        return [param.MepElement, param.StructureElement];
    }
}
