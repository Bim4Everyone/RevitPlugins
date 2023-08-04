using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;

using Ninject;

using RevitVolumeOfWork.Models;
using RevitVolumeOfWork.ViewModels;
using RevitVolumeOfWork.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitVolumeOfWork {
    [Transaction(TransactionMode.Manual)]
    public class RevitVolumeOfWorkCommand : BasePluginCommand {
        public RevitVolumeOfWorkCommand() {
            PluginName = "Характеристики ВОР Кладка";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = new StandardKernel()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();

                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());

                UpdateParams(uiApplication);

                MainWindow window = kernel.Get<MainWindow>();
                if(window.ShowDialog() == true) {
                    GetPlatformService<INotificationService>()
                        .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                        .ShowAsync();
                } else {
                    GetPlatformService<INotificationService>()
                        .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                        .ShowAsync();
                }
            }
        }

        private static void UpdateParams(UIApplication uiApplication) {
            ProjectParameters projectParameters = ProjectParameters.Create(uiApplication.Application);
            projectParameters.SetupRevitParams(uiApplication.ActiveUIDocument.Document,
                ProjectParamsConfig.Instance.RelatedRoomName,
                ProjectParamsConfig.Instance.RelatedRoomNumber,
                ProjectParamsConfig.Instance.RelatedRoomID,
                ProjectParamsConfig.Instance.RelatedRoomGroup);
        }
    }
}