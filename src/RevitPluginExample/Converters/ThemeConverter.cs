using dosymep.SimpleServices;

using Wpf.Ui.Appearance;

namespace RevitPluginExample.Converters {
    public class ThemeConverter {
        public static ApplicationTheme ConvertToApplicationTheme(UIThemes theme) {
            if(theme == UIThemes.Dark) {
                return ApplicationTheme.Dark;
            } else if(theme == UIThemes.Light) {
                return ApplicationTheme.Light;
            } else {
                return ApplicationTheme.Unknown;
            }
        }

        public static UIThemes ConvertToUIThemes(ApplicationTheme theme) {
            if(theme == ApplicationTheme.Dark) {
                return UIThemes.Dark;
            } else if(theme == ApplicationTheme.Light) {
                return UIThemes.Light;
            } else {
                return UIThemes.Light;
            }
        }
    }
}
