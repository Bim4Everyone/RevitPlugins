using RevitSleeves.Services.Placing.ParamsSetter;

namespace RevitSleeves.Services.Placing.ParamsSetterFinder;
internal interface IParamsSetterFinder<T> where T : class {
    IParamsSetter GetParamsSetter(T param);
}
