using System;
using System.Collections.Generic;
using System.Linq;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.PlacingOptsFinder;
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


    public SleevePlacingOpts GetOpts(T param) {
        return new SleevePlacingOpts() {
            FamilySymbol = _symbolFinder.GetFamilySymbol(param),
            Level = _levelFinder.GetLevel(param),
            Point = _pointFinder.GetPoint(param),
            Rotation = _rotationFinder.GetRotation(param),
            ParamsSetter = _paramsSetterFinder.GetParamsSetter(param)
        };
    }

    public ICollection<SleevePlacingOpts> GetOpts(ICollection<T> @params) {
        return [.. @params.Select(GetOpts)];
    }
}
