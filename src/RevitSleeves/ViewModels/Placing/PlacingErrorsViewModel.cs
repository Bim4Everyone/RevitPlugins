using System;
using System.Linq;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Services.Placing;

namespace RevitSleeves.ViewModels.Placing;
internal class PlacingErrorsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IPlacingErrorsService _placingErrorsService;
    private string _errors;

    public PlacingErrorsViewModel(
        ILocalizationService localizationService,
        IPlacingErrorsService placingErrorsService) {

        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _placingErrorsService = placingErrorsService ?? throw new ArgumentNullException(nameof(placingErrorsService));

        Errors = string.Join("\n\n", _placingErrorsService.GetAllErrors()
            .Select(m => new {
                Message = m.Message,
                Elements = string.Join("\n", m.GetDependentElements()
                    .Select(e => $"{e.Document.Title}: {e.Id.GetIdValue()}"))
            })
            .Select(m => $"{m.Message}:\n{m.Elements}"));
    }

    public string Errors {
        get => _errors;
        set => RaiseAndSetIfChanged(ref _errors, value);
    }
}
