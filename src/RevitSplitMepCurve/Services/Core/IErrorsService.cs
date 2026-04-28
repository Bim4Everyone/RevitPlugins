using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSplitMepCurve.Models.Errors;

namespace RevitSplitMepCurve.Services.Core;

internal interface IErrorsService {
    void AddError(ErrorModel error);

    void AddError(MEPCurve element, string localizationKey);

    ICollection<ErrorModel> GetErrors();

    bool ContainsErrors();

    void Clear();
}
