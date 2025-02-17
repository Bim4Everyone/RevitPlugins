using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitRoughFinishingDesign.Models;
using RevitRoughFinishingDesign.Services;
using RevitRoughFinishingDesign.ViewModels;
using RevitRoughFinishingDesign.Views;

namespace RevitRoughFinishingDesign {
    [Transaction(TransactionMode.Manual)]
    public class RevitRoughFinishingDesignCommand : IExternalCommand {
        public RevitRoughFinishingDesignCommand() {
            PluginName = "RevitRoughFinishingDesign";
        }

        public string PluginName { get; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIApplication uiApplication = commandData.Application;
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());
                kernel.Bind<ICurveLoopsSimplifier>()
                    .To<CurveLoopsSimplifier>()
                    .InSingletonScope();
                kernel.Bind<CreatesLinesForFinishing>()
                    .To<CreatesLinesForFinishing>()
                    .InSingletonScope();
                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                Notification(kernel.Get<MainWindow>());
                return Result.Succeeded;
            }
        }


        protected void Notification(Window window) {
            Notification(window.ShowDialog());
        }
        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        protected void Notification(bool? dialogResult) {
            if(dialogResult == null) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выход из скрипта.", "C#")
                    .ShowAsync();
            } else if(dialogResult == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else if(dialogResult == false) {
                throw new OperationCanceledException();
            }
        }

    }
}
