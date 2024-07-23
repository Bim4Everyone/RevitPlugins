using System;
using System.IO;
using System.Linq;
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
using RevitClashDetective.ViewModels.FilterCreatorViewModels;
using RevitClashDetective.Views;

namespace RevitClashDetective {

    [Transaction(TransactionMode.Manual)]
    public class CreateFiltersCommand : BasePluginCommand {
        public CreateFiltersCommand() {
            PluginName = "Поиск коллизий";
        }

        protected override void Execute(UIApplication uiApplication) {
            ExecuteCommand(uiApplication);
        }

        public void ExecuteCommand(UIApplication uiApplication, string selectedFilter = null) {
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
                        var path = Path.Combine(repo.GetObjectName(), repo.GetDocumentName());
                        return FiltersConfig.GetFiltersConfig(path, repo.Doc);
                    });

                kernel.Bind<FiltersViewModel>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<FilterCreatorView>()
                    .ToSelf()
                    .InSingletonScope()
                    .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<FiltersViewModel>());

                if(selectedFilter != null) {
                    var viewModel = kernel.Get<FiltersViewModel>();
                    viewModel.SelectedFilter = viewModel.Filters
                        .FirstOrDefault(item => item.Name
                        .Equals(selectedFilter, StringComparison.CurrentCultureIgnoreCase));
                }

                Notification(kernel.Get<FilterCreatorView>());
            }
        }
    }
}
