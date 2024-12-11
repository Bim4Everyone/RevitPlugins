using System.Windows;

using dosymep.SimpleServices;

using RevitPluginExample.Converters;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitPluginExample.Services {
    public class ThemeUpdaterService : IUIThemeUpdaterService {

        private ApplicationTheme _currentTheme = ApplicationThemeManager.GetAppTheme();

        public void SetTheme(Window window, UIThemes theme) {
            var appTheme = ThemeConverter.ConvertToApplicationTheme(theme);
            var newTheme = appTheme == ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark;
            if(_currentTheme != newTheme) {
                _currentTheme = newTheme;
                ApplicationThemeManager.Apply(_currentTheme, WindowBackdropType.Mica, true);
            }
        }
    }
}
