using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using Ninject;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.ViewModels;
using RevitArchitecturalDocumentation.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitArchitecturalDocumentation {
    [Transaction(TransactionMode.Manual)]
    public class RevitCreatingARDocsCommand : BasePluginCommand {
        public RevitCreatingARDocsCommand() {
            PluginName = "Создать документацию ПСО и ДДУ";
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

                kernel.Bind<MainOptions>()
                    .ToSelf();
                kernel.Bind<SheetOptions>()
                    .ToSelf();
                kernel.Bind<ViewOptions>()
                    .ToSelf();
                kernel.Bind<SpecOptions>()
                    .ToSelf();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<CreatingARDocsVM>().ToSelf();
                kernel.Bind<CreatingARDocsV>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<CreatingARDocsVM>());

                Notification(kernel.Get<CreatingARDocsV>());
            }
        }
    }
}
