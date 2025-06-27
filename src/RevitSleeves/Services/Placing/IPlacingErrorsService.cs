using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface IPlacingErrorsService {
    ICollection<ErrorModel> GetAllErrors();

    void AddError(ErrorModel error);

    void AddError(ICollection<Element> dependentElements, string localizationKey);

    bool ContainsErrors();
}
