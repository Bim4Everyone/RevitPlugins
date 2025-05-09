using System.Windows.Input;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using Ninject;
using Ninject.Syntax;

using RevitPlatformSettings.Views;

namespace RevitPlatformSettings.ViewModels.Settings;

internal sealed class AboutSettingsViewModel : SettingsViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IPlatformSettingsService _platformSettingsService;
    private readonly IResolutionRoot _resolutionRoot;

    public AboutSettingsViewModel(
        IResolutionRoot resolutionRoot,
        ILocalizationService localizationService,
        IPlatformSettingsService platformSettingsService) {
        _resolutionRoot = resolutionRoot;
        _localizationService = localizationService;
        _platformSettingsService = platformSettingsService;

        ImagePath = _platformSettingsService.CorpSettings?.ImagePath
                    ?? "/dosymep.Bim4Everyone;component/assets/Bim4Everyone.png";

        PlatformName = _platformSettingsService.CorpSettings?.Name;
        PlatformPageUrl = _platformSettingsService.SocialsSettings?.PlatformPageUrl;

        NewsChatUrl = _platformSettingsService.SocialsSettings?.NewsChatUrl;
        MainChatUrl = _platformSettingsService.SocialsSettings?.MainChatUrl;
        DownloadLinkUrl = _platformSettingsService.SocialsSettings?.DownloadLinkUrl;

        ShowOpenSourceDialogCommand = RelayCommand.Create(ShowOpenSourceDialog);
    }

    public ICommand ShowOpenSourceDialogCommand { get; }

    public string ImagePath { get; }
    public string PlatformName { get; }
    public string PlatformPageUrl { get; }

    public string NewsChatUrl { get; }
    public string MainChatUrl { get; }
    public string DownloadLinkUrl { get; }

    public string PlatformDescription =>
        _localizationService.GetLocalizedString("AboutSettings.PlatformDescription", PlatformName);

    private void ShowOpenSourceDialog() {
        _resolutionRoot.Get<OpenSourceWindow>().ShowDialog();
    }
}
