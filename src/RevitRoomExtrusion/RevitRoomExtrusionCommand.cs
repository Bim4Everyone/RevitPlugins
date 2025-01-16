using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitRoomExtrusion.Models;
using RevitRoomExtrusion.ViewModels;
using RevitRoomExtrusion.Views;

namespace RevitRoomExtrusion {
    [Transaction(TransactionMode.Manual)]
    public class RevitRoomExtrusionCommand : BasePluginCommand {
        public RevitRoomExtrusionCommand() {
            PluginName = "RevitRoomExtrusion";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                kernel.Bind<ErrorViewModel>().ToSelf();
                kernel.Bind<ErrorWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<ErrorViewModel>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                RoomChecker roomChecker = kernel.Get<RoomChecker>();

                if(roomChecker.IsSelected()) {
                    if(roomChecker.IsCheked()) {
                        Notification(kernel.Get<MainWindow>());
                    } else {
                        var errorWindow = kernel.Get<ErrorWindow>();
                        errorWindow.Show();
                        throw new OperationCanceledException();
                    }
                } else {
                    throw new OperationCanceledException();
                }
            }
        }
    }
}
