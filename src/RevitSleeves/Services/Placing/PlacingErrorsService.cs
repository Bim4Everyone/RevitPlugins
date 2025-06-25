using System.Collections.Generic;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal class PlacingErrorsService : IPlacingErrorsService {
    private readonly List<ErrorModel> _errors;

    public PlacingErrorsService() {
        _errors = [];
    }


    public void AddError(ErrorModel error) {
        _errors.Add(error);
    }

    public ICollection<ErrorModel> GetAllErrors() {
        return _errors;
    }
}
