using System;
using System.Globalization;

using dosymep.SimpleServices;

namespace RevitRefreshLinks.Services;

internal class HasLocalization : IHasLocalization {
    public HasLocalization(ILocalizationService localizationService, ILanguageService languageService) {
        LocalizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        LanguageService.LanguageChanged += _ => LanguageChanged?.Invoke(_);
    }

    public CultureInfo HostLanguage => LanguageService.HostLanguage;
    public ILocalizationService LocalizationService { get; }
    public ILanguageService LanguageService { get; }
    public event Action<CultureInfo> LanguageChanged;
}
