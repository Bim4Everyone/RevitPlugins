using System.Collections.Generic;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface IPlacingErrorsService {
    ICollection<ErrorModel> GetAllErrors();

    void AddError(ErrorModel error);
}
