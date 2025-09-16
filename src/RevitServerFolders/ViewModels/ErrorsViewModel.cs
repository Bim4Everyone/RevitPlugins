using System;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;

using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;
internal class ErrorsViewModel {
    private readonly ILocalizationService _localization;
    private readonly IErrorsService _errorsService;

    public ErrorsViewModel(ILocalizationService localization, IErrorsService errorsService) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));

        Errors = [.. _errorsService.GetAllErrors().Select(e => new ErrorViewModel(_localization, e))];
    }


    public ObservableCollection<ErrorViewModel> Errors { get; }
}
