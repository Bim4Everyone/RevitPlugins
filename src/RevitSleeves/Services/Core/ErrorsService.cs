using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class ErrorsService : IErrorsService {
    private readonly List<ErrorModel> _errors;
    private readonly ILocalizationService _localizationService;

    public ErrorsService(ILocalizationService localizationService) {
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        _errors = [];
    }


    public void AddError(ErrorModel error) {
        _errors.Add(error);
    }

    public void AddError(ICollection<Element> dependentElements, string localizationKey) {
        if(string.IsNullOrWhiteSpace(localizationKey)) {
            throw new ArgumentException(nameof(localizationKey));
        }

        _errors.Add(new ErrorModel(dependentElements, _localizationService.GetLocalizedString(localizationKey)));
    }

    public bool ContainsErrors() {
        return _errors.Count > 0;
    }

    public ICollection<ErrorModel> GetAllErrors() {
        return _errors;
    }
}
