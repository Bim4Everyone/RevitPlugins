
using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels;
internal class AddLocalLinksViewModel : BaseViewModel {
    private readonly ILocalSourceLinksProvider _linksProvider;
    private readonly ILocalizationService _localizationService;
    private readonly IProgressDialogFactory _progressDialogFactory;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILinksLoader _linksLoader;

    public AddLocalLinksViewModel(
        ILocalSourceLinksProvider linksProvider,
        ILocalizationService localizationService,
        IProgressDialogFactory progressDialogFactory,
        IMessageBoxService messageBoxService,
        ILinksLoader linksLoader) {

        _linksProvider = linksProvider
            ?? throw new System.ArgumentNullException(nameof(linksProvider));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        _progressDialogFactory =
            progressDialogFactory ?? throw new ArgumentNullException(nameof(progressDialogFactory));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _linksLoader = linksLoader
                       ?? throw new System.ArgumentNullException(nameof(linksLoader));
    }


    public bool ShowWindow() {
        var linksFromSource = _linksProvider.GetLocalLinks();
        ICollection<(ILink Link, string Error)> errors;
        using(var progressDialogService = _progressDialogFactory.CreateDialog()) {
            var progress = progressDialogService.CreateProgress();
            progressDialogService.MaxValue = linksFromSource.Links.Count;
            var ct = progressDialogService.CreateCancellationToken();
            progressDialogService.Show();

            errors = _linksLoader.AddLinks(linksFromSource.Links, progress, ct);
        }
        if(errors?.Count > 0) {
            string msg = string.Join("\n\n",
                errors.GroupBy(e => e.Error)
                .Select(e => $"{e.Key}:\n{string.Join("\n", e.Select(i => i.Link.Name))}"));
            _messageBoxService.Show(
                msg,
                _localizationService.GetLocalizedString("MessageBox.Title.ErrorAddLink"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
        return true;
    }
}
