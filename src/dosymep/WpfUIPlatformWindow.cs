﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Interop;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Serializers;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Behaviors;

using Microsoft.Xaml.Behaviors;

using pyRevitLabs.Json;

using Wpf.Ui.Controls;

namespace dosymep.WpfUI.Core {
    public class WpfUIPlatformWindow : FluentWindow, IHasTheme, IHasLocalization {
        public event Action<UIThemes> ThemeChanged;
        public event Action<CultureInfo> LanguageChanged;

        private readonly WindowInteropHelper _windowInteropHelper;

        public WpfUIPlatformWindow() { }

        public WpfUIPlatformWindow(
            ILoggerService loggerService,
            ISerializationService serializationService,
            ILanguageService languageService, ILocalizationService localizationService,
            IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) {
            LoggerService = loggerService;
            SerializationService = serializationService;

            LanguageService = languageService;
            LocalizationService = localizationService;

            UIThemeService = uiThemeService;
            ThemeUpdaterService = themeUpdaterService;

            UIThemeService.UIThemeChanged += _ => ThemeChanged?.Invoke(_);
            LanguageService.LanguageChanged += _ => LanguageChanged?.Invoke(_);

            _windowInteropHelper = new WindowInteropHelper(this) {Owner = Process.GetCurrentProcess().MainWindowHandle};

            Interaction.GetBehaviors(this).Add(new WpfThemeBehavior());
            Interaction.GetBehaviors(this).Add(new WpfLocalizationBehavior());
        }

        public ILoggerService LoggerService { get; }
        public ISerializationService SerializationService { get; }

        public ILanguageService LanguageService { get; }
        public ILocalizationService LocalizationService { get; }

        public IUIThemeService UIThemeService { get; }
        public IUIThemeUpdaterService ThemeUpdaterService { get; }

        public UIThemes HostTheme => UIThemeService.HostTheme;
        public CultureInfo HostLanguage => LanguageService.HostLanguage;

        /// <summary>
        /// Наименование плагина.
        /// </summary>
        public virtual string PluginName { get; } = "Platform";

        /// <summary>
        /// Наименование файла конфигурации.
        /// </summary>
        public virtual string ProjectConfigName { get; } = nameof(WpfUIPlatformWindow);

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            Interaction.GetBehaviors(this)
                .Add(new WpfSavePositionBehavior() {
                    SerializationService = SerializationService,
                    ConfigPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "dosymep", ModuleEnvironment.RevitVersion, PluginName, ProjectConfigName + ".json"),
                });
        }
    }
}
