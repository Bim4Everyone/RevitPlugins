using System;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.ViewModels;
using RevitSetLevelSection.Views;

namespace RevitSetLevelSection {
    [Transaction(TransactionMode.Manual)]
    public class SetLevelSectionCommand : BasePluginCommand {
        public SetLevelSectionCommand() {
            PluginName = "Назначение уровня/секции";
        }

        protected override void Execute(UIApplication uiApplication) {
            ProjectParameters projectParameters = ProjectParameters.Create(uiApplication.Application);
            projectParameters.SetupRevitParams(uiApplication.ActiveUIDocument.Document,
                SharedParamsConfig.Instance.BuildingWorksLevel,
                SharedParamsConfig.Instance.BuildingWorksBlock,
                SharedParamsConfig.Instance.BuildingWorksSection,
                SharedParamsConfig.Instance.BuildingWorksTyping,
                SharedParamsConfig.Instance.FixBuildingWorks);

            var repository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var viewModel = new MainViewModel(repository);

            var window = new MainWindow() { DataContext = viewModel };
            if(window.ShowDialog() == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }
    }
}
