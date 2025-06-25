using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Placing.LevelFinder;
using RevitSleeves.Services.Placing.ParamsSetterFinder;
using RevitSleeves.Services.Placing.PointFinder;
using RevitSleeves.Services.Placing.RotationFinder;

namespace RevitSleeves.Services.Placing.PlacingOptsProvider;
internal class PipeFloorPlacingOptsProvider : PlacingOptsProvider<ClashModel<Pipe, Floor>> {
    public PipeFloorPlacingOptsProvider(
        IFamilySymbolFinder<ClashModel<Pipe, Floor>> symbolFinder,
        ILevelFinder<ClashModel<Pipe, Floor>> levelFinder,
        IPointFinder<ClashModel<Pipe, Floor>> pointFinder,
        IRotationFinder<ClashModel<Pipe, Floor>> rotationFinder,
        IParamsSetterFinder<ClashModel<Pipe, Floor>> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }

    protected override Element[] GetDependentElements(ClashModel<Pipe, Floor> param) {
        return [param.MepElement, param.StructureElement];
    }
}
