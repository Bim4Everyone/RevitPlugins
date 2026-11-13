using System;

using dosymep.SimpleServices;

namespace RevitRefreshLinks.Services;

internal class HasTheme : IHasTheme {
    public HasTheme(IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) {
        UIThemeService = uiThemeService ?? throw new ArgumentNullException(nameof(uiThemeService));
        ThemeUpdaterService = themeUpdaterService ?? throw new ArgumentNullException(nameof(themeUpdaterService));
        UIThemeService.UIThemeChanged += _ => ThemeChanged?.Invoke(_);
    }

    public IUIThemeService UIThemeService { get; }
    public UIThemes HostTheme => UIThemeService.HostTheme;
    public IUIThemeUpdaterService ThemeUpdaterService { get; }
    public event Action<UIThemes> ThemeChanged;
}
