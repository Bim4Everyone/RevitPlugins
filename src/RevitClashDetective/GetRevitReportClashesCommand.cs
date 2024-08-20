using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective {
    //TODO эта команда для дебага и отладки логики выполнения плагина
    [Transaction(TransactionMode.Manual)]
    public class GetRevitReportClashesCommand : BasePluginCommand {
        public GetRevitReportClashesCommand() {
            PluginName = "Отчет Revit-а о пересечениях";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitEventHandler>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ParameterFilterProvider>()
                    .ToSelf()
                    .InSingletonScope();

                var repo = kernel.Get<RevitRepository>();
                var revitRepository = repo;
                var mainViewModlel = new RevitReportClashesViewModel(revitRepository);
                var window = new RevitReportClashNavigator() { DataContext = mainViewModlel };
                window.Show();
            }
        }
    }
}
