using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitBatchSpecExport.Models;
using RevitBatchSpecExport.Services;
using RevitBatchSpecExport.Services.Implements;
using RevitBatchSpecExport.ViewModels;
using RevitBatchSpecExport.Views;

namespace RevitBatchSpecExport;

[Transaction(TransactionMode.Manual)]
public class RevitBatchSpecExportCommand : BasePluginCommand {
    public RevitBatchSpecExportCommand() {
        PluginName = "Пакетный экспорт спек в XLSX";
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
        kernel.UseWpfOpenFileDialog<MainViewModel>(
            filter: "Revit projects |*.rvt",
            multiSelect: true,
            initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        kernel.UseWpfOpenFolderDialog<MainViewModel>(
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
