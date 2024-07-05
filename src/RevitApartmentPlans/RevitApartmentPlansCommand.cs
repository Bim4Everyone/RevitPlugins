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
                kernel.Bind<IBoundsCalculationService>()
                    .To<BoundsCalculationService>()
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
                kernel.Bind<IViewPlanCreationService>()
                    .To<ViewPlanCreationService>()
                    .InSingletonScope();
                kernel.Bind<ILengthConverter>()
                    .To<Services.LengthConverter>()
                    .InSingletonScope();
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<ApartmentsViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ViewTemplatesViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                //TODO Debug only code
                //var repo = kernel.Get<RevitRepository>();
                //var apartment = repo.GetDebugApartment();
                //var planCreationService = kernel.Get<IViewPlanCreationService>();
                //var lengthConverter = kernel.Get<ILengthConverter>();
                //planCreationService.CreateViews(
                //    new Apartment[] { apartment },
                //    repo.GetDebugTemplates(),
                //    lengthConverter.ConvertToInternal(200));
                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
