
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels {
    internal class AddLinksViewModel : BaseViewModel {
        private readonly IOneSourceLinksProvider _linksProvider;
        private readonly ILinksLoader _linksLoader;

        public AddLinksViewModel(
            IOneSourceLinksProvider linksProvider,
            ILinksLoader linksLoader) {

            _linksProvider = linksProvider
                ?? throw new System.ArgumentNullException(nameof(linksProvider));
            _linksLoader = linksLoader
                ?? throw new System.ArgumentNullException(nameof(linksLoader));
        }


        public bool ShowWindow() {
            var links = _linksProvider.GetLinks();
            ICollection<(ILink Link, string Error)> errors;
            using(var progressDialogService = GetPlatformService<IProgressDialogService>()) {
                var progress = progressDialogService.CreateProgress();
                progressDialogService.MaxValue = links.Count;
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                errors = _linksLoader.AddLinks(links, progress, ct);
            }
            if(errors.Count > 0) {
                var msg = string.Join("\n\n",
                    errors.GroupBy(e => e.Error)
                    .Select(e => $"{e.Key}:\n{string.Join("\n", e.Select(i => i.Link.Name))}"));
                GetPlatformService<IMessageBoxService>()
                    .Show(msg,
                    "Ошибки добавления связей",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
            return true;
        }
    }
}
