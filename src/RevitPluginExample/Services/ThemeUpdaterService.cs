using System.Windows;

using dosymep.SimpleServices;

using RevitPluginExample.Converters;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitPluginExample.Services {
    public class ThemeUpdaterService : IUIThemeUpdaterService {
        public void SetTheme(Window window, UIThemes theme) {
            if(theme == UIThemes.Dark) {
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            } else if(theme == UIThemes.Light) {
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
            }
        }
    }
}
