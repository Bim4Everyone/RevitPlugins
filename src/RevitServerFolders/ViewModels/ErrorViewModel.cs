using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;

namespace RevitServerFolders.ViewModels;
internal class ErrorViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;

    public ErrorViewModel(ILocalizationService localization, ErrorModel error) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        ErrorModel = error ?? throw new ArgumentNullException(nameof(error));

        GroupHeader = _localization.GetLocalizedString("ErrorsWindow.ErrorGroupHeader", error.ExportSettings.Index);
        ModelName = error.ModelName;
        ErrorDescription = error.ErrorDescription;
    }


    public string GroupHeader { get; }

    public string ModelName { get; }

    public string ErrorDescription { get; }

    public ErrorModel ErrorModel { get; }
}
