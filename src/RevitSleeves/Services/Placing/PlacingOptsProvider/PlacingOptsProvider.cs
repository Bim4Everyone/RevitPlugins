using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSleeves.Exceptions;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Placing.LevelFinder;
using RevitSleeves.Services.Placing.ParamsSetterFinder;
using RevitSleeves.Services.Placing.PointFinder;
using RevitSleeves.Services.Placing.RotationFinder;

namespace RevitSleeves.Services.Placing.PlacingOptsProvider;
internal abstract class PlacingOptsProvider<T> : IPlacingOptsProvider<T> where T : class {
    private readonly IFamilySymbolFinder<T> _symbolFinder;
    private readonly ILevelFinder<T> _levelFinder;
    private readonly IPointFinder<T> _pointFinder;
    private readonly IRotationFinder<T> _rotationFinder;
    private readonly IParamsSetterFinder<T> _paramsSetterFinder;

    protected PlacingOptsProvider(
        IFamilySymbolFinder<T> symbolFinder,
        ILevelFinder<T> levelFinder,
        IPointFinder<T> pointFinder,
        IRotationFinder<T> rotationFinder,
        IParamsSetterFinder<T> paramsSetterFinder) {

        _symbolFinder = symbolFinder
            ?? throw new ArgumentNullException(nameof(symbolFinder));
        _levelFinder = levelFinder
            ?? throw new ArgumentNullException(nameof(levelFinder));
        _pointFinder = pointFinder
            ?? throw new ArgumentNullException(nameof(pointFinder));
        _rotationFinder = rotationFinder
            ?? throw new ArgumentNullException(nameof(rotationFinder));
        _paramsSetterFinder = paramsSetterFinder
            ?? throw new ArgumentNullException(nameof(paramsSetterFinder));
    }


    public ICollection<SleevePlacingOpts> GetOpts(ICollection<T> @params) {
        return [.. @params.Select(GetOpts).Where(opts => opts is not null)];
    }

    protected abstract Element[] GetDependentElements(T param);

    private SleevePlacingOpts GetOpts(T param) {
        try {
            return new SleevePlacingOpts() {
                FamilySymbol = _symbolFinder.GetFamilySymbol(param),
                Level = _levelFinder.GetLevel(param),
                Point = _pointFinder.GetPoint(param),
                Rotation = _rotationFinder.GetRotation(param),
                ParamsSetter = _paramsSetterFinder.GetParamsSetter(param),
                DependentElements = GetDependentElements(param)
            };
        } catch(CannotCreateSleeveException) {
            return null;
        }
    }
}
