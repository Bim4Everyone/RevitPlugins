using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSplitMepCurve.Models.Errors;

namespace RevitSplitMepCurve.Services.Core;

internal class ErrorsService : IErrorsService {
    private readonly List<ErrorModel> _errors;
    private readonly ILocalizationService _localizationService;

    public ErrorsService(ILocalizationService localizationService) {
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        _errors = [];
    }

    public void AddError(ErrorModel error) {
        if(error == null) {
            throw new ArgumentNullException(nameof(error));
        }

        _errors.Add(error);
    }

    public void AddError(Element element, string localizationKey) {
        if(element == null) {
            throw new ArgumentNullException(nameof(element));
        }

        if(string.IsNullOrWhiteSpace(localizationKey)) {
            throw new ArgumentException(nameof(localizationKey));
        }
        _errors.Add(new ErrorModel(element, _localizationService.GetLocalizedString(localizationKey)));
    }

    public ICollection<ErrorModel> GetErrors() => _errors;

    public bool ContainsErrors() => _errors.Count > 0;

    public void Clear() => _errors.Clear();
}
