using System;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.Xpf.Core.Ninject;

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
                kernel.Bind<ICurveLoopsSimplifier>()
                    .To<CurveLoopsSimplifier>()
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

                kernel.UseXtraProgressDialog<MainViewModel>();
                kernel.UseXtraMessageBox<MainViewModel>();
                kernel.Bind<ApartmentsViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ViewTemplatesViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ViewTemplateAdditionViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ViewTemplateAdditionWindow>()
                    .ToSelf()
                    .InTransientScope()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<ViewTemplateAdditionViewModel>())
                    .WithPropertyValue(nameof(Window.Owner), c => c.Kernel.Get<MainWindow>());
                kernel.Bind<MainViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                var repo = kernel.Get<RevitRepository>();
                var msg = kernel.Get<IMessageBoxService>();
                CheckViews(repo, msg);
                Notification(kernel.Get<MainWindow>());
            }
        }


        private void CheckViews(RevitRepository revitRepository, IMessageBoxService messageBoxService) {
            if(!revitRepository.ActiveViewIsPlan()) {
                var result = messageBoxService.Show(
                    "Активный вид должен быть планом",
                    PluginName,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                throw new OperationCanceledException();
            }
        }
    }
}
