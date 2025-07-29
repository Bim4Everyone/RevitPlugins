using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitMepTotals.Models;
using RevitMepTotals.Services;
using RevitMepTotals.Services.Implements;
using RevitMepTotals.ViewModels;
using RevitMepTotals.Views;

namespace RevitMepTotals;
/// <summary>
/// Команда для выбора моделей Revit и последующей выгрузки информации из них в Excel файл.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RevitMepTotalsCommand : BasePluginCommand {
    public RevitMepTotalsCommand() {
        PluginName = "Выгрузить объемы";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<IDocumentsProcessor>()
            .To<DocumentsProcessor>()
            .InSingletonScope();
        kernel.Bind<IDataExporter>()
            .To<DataExporter>()
            .InSingletonScope();
        kernel.Bind<ICopyNameProvider>()
            .To<CopyNameProvider>()
            .InSingletonScope();
        kernel.Bind<IConstantsProvider>()
            .To<ConstantsProvider>()
            .InSingletonScope();
        kernel.Bind<IErrorMessagesProvider>()
            .To<ErrorMessagesProvider>()
            .InSingletonScope();

        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.UseWpfUIProgressDialog<MainViewModel>();
        kernel.UseXtraOpenFileDialog<MainViewModel>(
            filter: "Revit projects |*.rvt",
            multiSelect: true,
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        kernel.UseXtraOpenFolderDialog<MainViewModel>(
            multiSelect: false,
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        kernel.UseWpfUIMessageBox<MainViewModel>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }
}
