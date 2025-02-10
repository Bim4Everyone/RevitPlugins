using System;
using System.Globalization;
using System.Linq;
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
                ValidateCategories(revitRepository, localizationService);
                Notification(kernel.Get<MainWindow>());
            }
        }

        private void ValidateSelectedElements(RevitRepository revitRepository, ILocalizationService localizationService) {
            var selectedElement = revitRepository.GetSelectedElements();
            if(selectedElement.Count == 0) {
                ShowError(localizationService, "GeneralSettings.ErrorNoSelectedElements");
            }
        }

        private void ValidateCategories(RevitRepository revitRepository, ILocalizationService localizationService) {
            var validCategories = revitRepository.GetCategoriesWithMarkParam(BuiltInParameter.ALL_MODEL_MARK);

            if(!validCategories.Any()) {
                ShowError(localizationService, "GeneralSettings.CategoryMismatch");
            }
        }

        private void ShowError(ILocalizationService localizationService, string messageKey) {
            string title = localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
            string message = localizationService.GetLocalizedString(messageKey);

            TaskDialog.Show(title, message);
            throw new OperationCanceledException();
        }
    }
}
