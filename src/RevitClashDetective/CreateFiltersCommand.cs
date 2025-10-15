using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.ViewModels.FilterCreatorViewModels;
using RevitClashDetective.Views;

namespace RevitClashDetective;

[Transaction(TransactionMode.Manual)]
public class CreateFiltersCommand : BasePluginCommand {
    public CreateFiltersCommand() {
        PluginName = "Поисковые наборы";
    }

    protected override void Execute(UIApplication uiApplication) {
        ExecuteCommand(uiApplication);
    }

    public void ExecuteCommand(UIApplication uiApplication, string selectedFilter = null) {
        using var kernel = uiApplication.CreatePlatformServices();
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
                string path = Path.Combine(repo.GetObjectName(), repo.GetDocumentName());
                return FiltersConfig.GetFiltersConfig(path, repo.Doc);
            });

        kernel.Bind<FilterNameView>()
            .ToSelf()
            .InTransientScope();

        kernel.BindMainWindow<FiltersViewModel, FilterCreatorView>();
        kernel.UseWpfOpenFileDialog<FiltersViewModel>()
            .UseWpfSaveFileDialog<FiltersViewModel>()
            .UseWpfUIMessageBox<FiltersViewModel>();

        kernel.UseWpfUIThemeUpdater();
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        if(selectedFilter != null) {
            var viewModel = kernel.Get<FiltersViewModel>();
            viewModel.SelectedFilter = viewModel.Filters
                .FirstOrDefault(item => item.Name
                .Equals(selectedFilter, StringComparison.CurrentCultureIgnoreCase));
        }

        Notification(kernel.Get<FilterCreatorView>());
    }
}
