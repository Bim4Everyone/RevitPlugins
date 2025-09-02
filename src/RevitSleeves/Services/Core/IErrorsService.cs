using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal interface IErrorsService {
    ICollection<ErrorModel> GetAllErrors();

    void AddError(ErrorModel error);

    void AddError(ICollection<Element> dependentElements, string localizationKey);

    bool ContainsErrors();
}
