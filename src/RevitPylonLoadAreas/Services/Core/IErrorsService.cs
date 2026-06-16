using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Errors;

namespace RevitPylonLoadAreas.Services.Core;

internal interface IErrorsService {
    void AddError(ErrorModel error);

    void AddError(Element element, string localizationKey);

    ICollection<ErrorModel> GetErrors();

    bool ContainsErrors();

    void Clear();
}
