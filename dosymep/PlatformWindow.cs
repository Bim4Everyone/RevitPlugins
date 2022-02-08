using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace dosymep.WPF.Views {
    public class PlatformWindow : Window {
        /// <summary>
        /// Наименование плагина.
        /// </summary>
        public virtual string PluginName { get; }

        /// <summary>
        /// Наименование файла конфигурации.
        /// </summary>
        public virtual string ProjectConfigName { get; }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            PlatformWindowConfig config = GetProjectConfig();
            if(config.Maximized.HasValue) {
                Top = config.Top ?? Top;
                Left = config.Left ?? Left;

                if(ResizeMode == ResizeMode.CanResize) {
                    Width = config.Width ?? Width;
                    Height = config.Height ?? Height;
                }

                if(!config.Maximized.Value) {
                    SizeToFit();
                    MoveIntoView();
                }

                WindowState = config.Maximized.Value ? WindowState.Maximized : WindowState;
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            var config = GetProjectConfig();
            config.Top = Top;
            config.Left = Left;

            config.Width = ResizeMode == ResizeMode.CanResize ? Width : (double?) null;
            config.Height = ResizeMode == ResizeMode.CanResize ? Height : (double?) null;

            config.Maximized = WindowState == WindowState.Maximized;
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

        protected virtual void SizeToFit() {
            if(Height > System.Windows.SystemParameters.VirtualScreenHeight) {
                Height = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if(Width > System.Windows.SystemParameters.VirtualScreenWidth) {
                Width = System.Windows.SystemParameters.VirtualScreenWidth;
            }
        }

        protected virtual void MoveIntoView() {
            if(Top + Height / 2 >
               System.Windows.SystemParameters.VirtualScreenHeight) {
                Top =
                    System.Windows.SystemParameters.VirtualScreenHeight -
                    Height;
            }

            if(Left + Width / 2 >
               System.Windows.SystemParameters.VirtualScreenWidth) {
                Left =
                    System.Windows.SystemParameters.VirtualScreenWidth -
                    Width;
            }

            if(Top < 0) {
                Top = 0;
            }

            if(Left < 0) {
                Left = 0;
            }
        }
    }

    public class PlatformWindowConfig : ProjectConfig {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }
        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public double? Top { get; set; }
        public double? Left { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public bool? Maximized { get; set; }
    }
}