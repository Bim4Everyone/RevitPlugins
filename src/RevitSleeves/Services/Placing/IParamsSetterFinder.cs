namespace RevitSleeves.Services.Placing;
internal interface IParamsSetterFinder<T> where T : class {
    IParamsSetter GetParamsSetter(T param);
}
