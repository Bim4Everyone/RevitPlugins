using System;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitSectionsConstructor.Models;
using RevitSectionsConstructor.Services;
using RevitSectionsConstructor.ViewModels;
using RevitSectionsConstructor.Views;

namespace RevitSectionsConstructor {
    [Transaction(TransactionMode.Manual)]
    public class RevitSectionsConstructorCommand : BasePluginCommand {
        public RevitSectionsConstructorCommand() {
            PluginName = "Конструктор секций";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<GroupsHandler>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<DocumentSaver>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.UseXtraSaveFileDialog<MainViewModel>(
                    filter: "Revit projects |*.rvt",
                    initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    );


                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
