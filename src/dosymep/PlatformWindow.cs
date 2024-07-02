using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Serializers;
using dosymep.SimpleServices;

using pyRevitLabs.Json;

namespace dosymep.WPF.Views {
    public class PlatformWindow : Window {
        private readonly WindowInteropHelper _windowInteropHelper;
        public PlatformWindow() {
            LanguageService = GetPlatformService<ILanguageService>();
            
            _windowInteropHelper = new WindowInteropHelper(this) {
                Owner = Process.GetCurrentProcess().MainWindowHandle
            };
        }
        /// <summary>
        /// Наименование плагина.
        /// </summary>
        public virtual string PluginName { get; }

        /// <summary>
        /// Наименование файла конфигурации.
        /// </summary>
        public virtual string ProjectConfigName { get; }
        
        /// <summary>
        /// Сервис локализации окон.
        /// </summary>
        public virtual ILocalizationService LocalizationService { get; set; }

        /// <summary>
        /// Предоставляет доступ к логгеру платформы.
        /// </summary>
        protected ILoggerService LoggerService => GetPlatformService<ILoggerService>();
        
        /// <summary>
        /// Предоставляет доступ к текущему языку платформы.
        /// </summary>
        protected ILanguageService LanguageService { get; }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            LocalizationService?.SetLocalization(LanguageService.HostLanguage, this);
            
            base.OnSourceInitialized(e);

            PlatformWindowConfig config = GetProjectConfig();
            if(config.WindowPlacement.HasValue) {
                this.SetPlacement(config.WindowPlacement.Value);
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            PlatformWindowConfig config = GetProjectConfig();
            config.WindowPlacement = this.GetPlacement();
            config.SaveProjectConfig();
        }

        protected virtual PlatformWindowConfig GetProjectConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(PluginName)
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(ProjectConfigName + ".json")
                .Build<PlatformWindowConfig>();
        }
    }

    public class PlatformWindowConfig : ProjectConfig {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }
        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public WINDOWPLACEMENT? WindowPlacement { get; set; }
    }

    public static class UnsafeNativeMethods {
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        public static WINDOWPLACEMENT GetPlacement(this Window window) {
            return GetPlacement(new WindowInteropHelper(window).Handle);
        }

        public static void SetPlacement(this Window window, WINDOWPLACEMENT placement) {
            SetPlacement(new WindowInteropHelper(window).Handle, placement);
        }

        private static WINDOWPLACEMENT GetPlacement(IntPtr windowHandle) {
            GetWindowPlacement(windowHandle, out WINDOWPLACEMENT placement);
            return placement;
        }

        private static void SetPlacement(IntPtr windowHandle, WINDOWPLACEMENT placement) {
            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            placement.flags = 0;
            placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
            SetWindowPlacement(windowHandle, ref placement);
        }
    }

    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom) {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int X;
        public int Y;

        public POINT(int x, int y) {
            X = x;
            Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }
}
