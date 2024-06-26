using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.Models.Analyzers;
using RevitReinforcementCoefficient.Models.ElementModels;
using RevitReinforcementCoefficient.Models.Report;
using RevitReinforcementCoefficient.ViewModels;
using RevitReinforcementCoefficient.Views;

namespace RevitReinforcementCoefficient {
    [Transaction(TransactionMode.Manual)]
    public class RevitReinforcementCoefficientCommand : BasePluginCommand {
        public RevitReinforcementCoefficientCommand() {
            PluginName = "Подсчет коэффициентов армирования";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {

                kernel.Bind<IReportService>()
                    .To<ReportService>()
                    .InSingletonScope();

                kernel.Bind<ReportViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ReportWindow>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<ReportViewModel>());

                kernel.Bind<ParamUtils>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ElementFactory>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<DesignTypeAnalyzer>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<DesignTypeListAnalyzer>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<DesignTypeListVM>()
                    .ToSelf()
                    .InSingletonScope();

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

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
