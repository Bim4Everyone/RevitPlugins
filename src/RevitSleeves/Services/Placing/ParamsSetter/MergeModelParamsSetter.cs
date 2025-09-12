using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.PointFinder;

namespace RevitSleeves.Services.Placing.ParamsSetter;
internal class MergeModelParamsSetter : ParamsSetter, IParamsSetter<SleeveMergeModel> {
    private readonly IErrorsService _errorsService;
    private readonly ILocalizationService _localizationService;
    private readonly IPointFinder<SleeveMergeModel> _pointFinder;
    private readonly SleeveMergeModel _sleeveModel;

    public MergeModelParamsSetter(
        IErrorsService errorsService,
        ILocalizationService localizationService,
        IPointFinder<SleeveMergeModel> pointFinder,
        SleeveMergeModel sleeveModel) {

        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _sleeveModel = sleeveModel ?? throw new ArgumentNullException(nameof(sleeveModel));
    }


    public void SetParamValues(FamilyInstance sleeve) {
        SetElevation(sleeve, _pointFinder.GetPoint(_sleeveModel));
        var orientation = _sleeveModel.GetOrientation();
        double angle = Math.Asin(orientation.Z);
        SetInclineAngle(sleeve, angle);
        SetDiameter(sleeve, _sleeveModel.GetDiameter());
        (var start, var end) = _sleeveModel.GetEndPoints();
        double length = (end - start).GetLength();
        SetLength(sleeve, length);
        SetStringParameters(sleeve, _sleeveModel);
    }

    protected void SetStringParameters(FamilyInstance sleeve, SleeveMergeModel mergeModel) {
        var firstMergeSleeve = mergeModel.GetSleeves().First().GetFamilyInstance();
        sleeve.SetParamValue(NamesProvider.ParameterSleeveSystem,
            firstMergeSleeve.GetParamValue<string>(NamesProvider.ParameterSleeveSystem));
        sleeve.SetParamValue(NamesProvider.ParameterSleeveEconomic,
            firstMergeSleeve.GetParamValue<string>(NamesProvider.ParameterSleeveEconomic));
        sleeve.SetParamValue(NamesProvider.ParameterSleeveDescription,
            firstMergeSleeve.GetParamValue<string>(NamesProvider.ParameterSleeveDescription));
    }
}
