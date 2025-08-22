using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitSectionsConstructor.Models;
using RevitSectionsConstructor.Services;
using RevitSectionsConstructor.ViewModels;
using RevitSectionsConstructor.Views;

namespace RevitSectionsConstructor;
[Transaction(TransactionMode.Manual)]
public class RevitSectionsConstructorCommand : BasePluginCommand {
    public RevitSectionsConstructorCommand() {
        PluginName = "Конструктор секций";
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<GroupsHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<DocumentSaver>()
            .ToSelf()
            .InSingletonScope();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.UseWpfUIMessageBox<MainViewModel>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var localization = kernel.Get<ILocalizationService>();

        kernel.UseXtraSaveFileDialog<MainViewModel>(
            title: localization.GetLocalizedString("SaveFileDialog.Title"),
            addExtension: true,
            filter: "Revit projects |*.rvt",
            defaultExt: "rvt");

        CheckViews(kernel.Get<RevitRepository>(), kernel.Get<IMessageBoxService>(), localization);

        Notification(kernel.Get<MainWindow>());
    }

    private void CheckViews(
        RevitRepository revitRepository,
        IMessageBoxService messageBoxService,
        ILocalizationService localization) {

        if(!revitRepository.ActiveDocOnEmptySheet()) {
            var result = messageBoxService.Show(localization.GetLocalizedString("ViewsWarning"),
                localization.GetLocalizedString("MainWindow.Title"),
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);
            if(result != System.Windows.MessageBoxResult.Yes) {
                throw new OperationCanceledException();
            }
        }
    }
}
