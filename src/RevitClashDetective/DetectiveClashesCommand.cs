using System.IO;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.ViewModels.ClashDetective;
using RevitClashDetective.Views;

namespace RevitClashDetective {

    [Transaction(TransactionMode.Manual)]
    public class DetectiveClashesCommand : BasePluginCommand {
        public DetectiveClashesCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication) {
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
                kernel.Bind<FiltersConfig>()
                    .ToMethod(c => {
                        var repo = c.Kernel.Get<RevitRepository>();
                        var path = GetConfigPath(repo);
                        return FiltersConfig.GetFiltersConfig(path, repo.Doc);
                    });
                kernel.Bind<ChecksConfig>()
                    .ToMethod(c => {
                        var repo = c.Kernel.Get<RevitRepository>();
                        var path = GetConfigPath(repo);
                        return ChecksConfig.GetChecksConfig(path, repo.Doc);
                    });

                kernel.Bind<MainViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }

        private string GetConfigPath(RevitRepository revitRepository) {
            return Path.Combine(revitRepository.GetObjectName(), revitRepository.GetDocumentName());
        }
    }
}
