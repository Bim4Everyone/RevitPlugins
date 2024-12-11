using System;

using dosymep.SimpleServices;

using Microsoft.Win32;

using RevitPluginExample.Converters;

using Wpf.Ui.Appearance;

namespace RevitPluginExample.Services {
    public class ThemeService : IUIThemeService, IDisposable {

        private readonly ApplicationTheme _currentTheme = ApplicationThemeManager.GetAppTheme();

        public event Action<UIThemes> UIThemeChanged;

        public ThemeService() {
            SystemEvents.UserPreferenceChanged += OnSystemEventsOnUserPreferenceChanged;
        }

        public UIThemes HostTheme => ThemeConverter.ConvertToUIThemes(_currentTheme);

        private void OnSystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) {
            if(e.Category == UserPreferenceCategory.General) {
                UIThemeChanged?.Invoke(HostTheme);
            }
        }

        private static UIThemes GetThemeFromWindows() {
            using(var registry = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")) {
                int? isLight = (int?) registry?.GetValue("AppsUseLightTheme");
                return isLight == 1 ? UIThemes.Light : UIThemes.Dark;
            }
        }

        /// <summary>
        /// Очищает подписку на события.
        /// </summary>
        /// <param name="disposing">Указывает, выполняется ли очистка ресурсов.</param>
        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                SystemEvents.UserPreferenceChanged -= OnSystemEventsOnUserPreferenceChanged;
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
