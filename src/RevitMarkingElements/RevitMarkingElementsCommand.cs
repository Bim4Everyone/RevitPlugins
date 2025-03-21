using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitMarkingElements.Models;
using RevitMarkingElements.ViewModels;
using RevitMarkingElements.Views;

namespace RevitMarkingElements {
    [Transaction(TransactionMode.Manual)]
    public class RevitMarkingElementsCommand : BasePluginCommand {
        public RevitMarkingElementsCommand() {
            PluginName = "Маркировка элементов";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                Document document = uiApplication.ActiveUIDocument.Document;
                View activeView = document.ActiveView;


                kernel.Bind<RevitRepository>().ToSelf().InSingletonScope();
                kernel.Bind<PluginConfig>().ToMethod(c => PluginConfig.GetPluginConfig());
                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

                var revitRepository = kernel.Get<RevitRepository>();
                var localizationService = kernel.Get<ILocalizationService>();
                ValidateSelectedElements(revitRepository, localizationService);
                Notification(kernel.Get<MainWindow>());
            }
        }

        private void ValidateSelectedElements(RevitRepository revitRepository, ILocalizationService localizationService) {
            var selectedElement = revitRepository.GetSelectedElements();

            if(selectedElement.Count == 0) {
                string title = localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
                string message = localizationService.GetLocalizedString("GeneralSettings.ErrorNoSelectedElements");

                TaskDialog.Show(title, message);
                throw new OperationCanceledException();
            }
        }
    }
}
