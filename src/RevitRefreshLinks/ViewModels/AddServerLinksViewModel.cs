using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels;
internal class AddServerLinksViewModel : BaseViewModel {
    private readonly IServerSourceLinksProvider _linksProvider;
    private readonly ILocalizationService _localizationService;
    private readonly ILinksLoader _linksLoader;
    private readonly IProgressDialogFactory _progressFactory;

    public AddServerLinksViewModel(
        IServerSourceLinksProvider linksProvider,
        ILocalizationService localizationService,
        ILinksLoader linksLoader,
        IProgressDialogFactory progressFactory) {

        _linksProvider = linksProvider
            ?? throw new System.ArgumentNullException(nameof(linksProvider));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        _linksLoader = linksLoader
            ?? throw new System.ArgumentNullException(nameof(linksLoader));
        _progressFactory = progressFactory
            ?? throw new System.ArgumentNullException(nameof(progressFactory));
        AddLinksCommand = RelayCommand.CreateAsync(AddLinks);
    }

    public ICommand AddLinksCommand { get; }

    public async Task AddLinks() {
        var linksFromSource = await _linksProvider.GetServerLinksAsync();
        ICollection<(ILink Link, string Error)> errors;
        using(var progressDialogService = _progressFactory.CreateDialog()) {
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
            GetPlatformService<IMessageBoxService>()
                .Show(msg,
                _localizationService.GetLocalizedString("MessageBox.Title.ErrorAddLink"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }
}
