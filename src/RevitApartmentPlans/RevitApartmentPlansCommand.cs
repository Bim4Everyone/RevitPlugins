using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitApartmentPlans.Models;
using RevitApartmentPlans.Services;
using RevitApartmentPlans.ViewModels;
using RevitApartmentPlans.Views;

namespace RevitApartmentPlans {
    [Transaction(TransactionMode.Manual)]
    public class RevitApartmentPlansCommand : BasePluginCommand {
        public RevitApartmentPlansCommand() {
            PluginName = "Планы квартир";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<IBoundsCalculateService>()
                    .To<BoundsCalculateService>()
                    .InSingletonScope();
                kernel.Bind<ICurveLoopsMerger>()
                    .To<CurveLoopsMerger>()
                    .InSingletonScope();
                kernel.Bind<ICurveLoopsOffsetter>()
                    .To<CurveLoopsOffsetter>()
                    .InSingletonScope();
                kernel.Bind<IRectangleLoopProvider>()
                    .To<RectangleLoopProvider>()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                //TODO Debug only code
                var repo = kernel.Get<RevitRepository>();
                var apartment = repo.GetDebugApartment();
                var service = kernel.Get<IBoundsCalculateService>();
                var loop = service.CreateBounds(apartment, 0);
                repo.CreateDebugLines(loop);
                //Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
