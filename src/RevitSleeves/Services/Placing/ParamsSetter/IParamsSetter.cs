using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing.ParamsSetter;

public interface IParamsSetter<T> : IParamsSetter where T : class {
}

public interface IParamsSetter {
    void SetParamValues(FamilyInstance sleeve);
}
