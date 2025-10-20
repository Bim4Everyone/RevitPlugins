using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using Ninject;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.ViewModels;
using RevitArchitecturalDocumentation.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitArchitecturalDocumentation;
[Transaction(TransactionMode.Manual)]
public class RevitCopySpecSheetInstanceCommand : BasePluginCommand {
    public RevitCopySpecSheetInstanceCommand() {
        PluginName = "Скопировать спецификации на листы";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = new StandardKernel();
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
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<CopySpecSheetInstanceVM>().ToSelf();
        kernel.Bind<CopySpecSheetInstanceV>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<CopySpecSheetInstanceVM>());

        Notification(kernel.Get<CopySpecSheetInstanceV>());
    }
}
