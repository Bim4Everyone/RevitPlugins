using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Placing.LevelFinder;
using RevitSleeves.Services.Placing.ParamsSetterFinder;
using RevitSleeves.Services.Placing.PointFinder;
using RevitSleeves.Services.Placing.RotationFinder;

namespace RevitSleeves.Services.Placing.PlacingOptsProvider;
internal class PipeOpeningPlacingOptsProvider : PlacingOptsProvider<ClashModel<Pipe, FamilyInstance>> {
    public PipeOpeningPlacingOptsProvider(
        IFamilySymbolFinder<ClashModel<Pipe, FamilyInstance>> symbolFinder,
        ILevelFinder<ClashModel<Pipe, FamilyInstance>> levelFinder,
        IPointFinder<ClashModel<Pipe, FamilyInstance>> pointFinder,
        IRotationFinder<ClashModel<Pipe, FamilyInstance>> rotationFinder,
        IParamsSetterFinder<ClashModel<Pipe, FamilyInstance>> paramsSetterFinder)
        : base(symbolFinder, levelFinder, pointFinder, rotationFinder, paramsSetterFinder) {
    }

    protected override Element[] GetDependentElements(ClashModel<Pipe, FamilyInstance> param) {
        return [param.MepElement, param.StructureElement];
    }
}
