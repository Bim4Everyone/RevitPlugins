using System;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;


using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitMepTotals.Models;
using RevitMepTotals.Models.Interfaces;
using RevitMepTotals.Services;
using RevitMepTotals.Services.Implements;
using RevitMepTotals.ViewModels;
using RevitMepTotals.Views;

namespace RevitMepTotals {
    /// <summary>
    /// ������� ��� ������ ������� Revit � ����������� �������� ���������� �� ��� � Excel ����.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class RevitMepTotalsCommand : BasePluginCommand {
        public RevitMepTotalsCommand() {
            PluginName = "��������� ������";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<IDocument>().To<RevitDocument>();
                kernel.Bind<IDocumentsProcessor>().To<DocumentsProcessor>();
                kernel.Bind<IDataExporter>().To<DataExporter>();
                kernel.Bind<ICopyNameProvider>().To<CopyNameProvider>();
                kernel.Bind<IDirectoryProvider>().To<DirectoryProvider>();
                kernel.UseXtraProgressDialog<MainViewModel>();
                kernel.UseXtraOpenFileDialog<MainViewModel>(
                    filter: "Revit projects |*.rvt",
                    multiSelect: true,
                    initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    );
                kernel.UseXtraMessageBox<MainViewModel>();

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
