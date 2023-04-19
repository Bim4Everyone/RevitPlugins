#region Namespaces
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using Ninject;

using PlatformSettings.Factories;
using PlatformSettings.Legacy;
using PlatformSettings.Model;
using PlatformSettings.Services;
using PlatformSettings.ViewModels;
using PlatformSettings.Views;

using pyRevitLabs.NLog;
using pyRevitLabs.PyRevit;

using Application = Autodesk.Revit.ApplicationServices.Application;

#endregion

namespace PlatformSettings {
    [Transaction(TransactionMode.Manual)]
    public class PlatformSettingsCommand : BasePluginCommand {
        public PlatformSettingsCommand() {
            PluginName = "Настройки платформы";
        }

        protected override void Execute(UIApplication uiApplication) {
            Notification(OpenSettingsWindow(uiApplication));
        }

        public bool? OpenSettingsWindow(UIApplication uiApplication) {
            using(IKernel kernel = new StandardKernel()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();
                
                
                kernel.Bind<BuiltinExtension>().ToSelf();
                kernel.Bind<ThirdPartyExtension>().ToSelf();

                kernel.Bind<IExtensionFactory<BuiltinExtension>>()
                    .To<ExtensionFactory<BuiltinExtension>>();
                
                kernel.Bind<IExtensionFactory<ThirdPartyExtension>>()
                    .To<ExtensionFactory<ThirdPartyExtension>>();
                
                kernel.Bind<IExtensionsService<BuiltinExtension>>()
                    .To<BuiltinExtensionsService>();
                
                kernel.Bind<IExtensionsService<ThirdPartyExtension>>()
                    .To<ThirdPartyExtensionsService>();
                
                kernel.Bind<IExtensionViewModelFactory>()
                    .To<ExtensionViewModelFactory>();
                
                kernel.Bind<ISettingsViewModelFactory>()
                    .To<SettingsViewModelFactory>();
                
                kernel.Bind<SettingsViewModel>().ToSelf();
                kernel.Bind<ExtensionsSettingsViewModel>().ToSelf();

                kernel.Bind<MainViewModel>().ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());

                return kernel.Get<MainWindow>().ShowDialog();
            }
        }
    }
}
