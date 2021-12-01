using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitFamilyExplorer.ViewModels;
using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class RegisterFamilyExplorerCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Execute(commandData.Application);
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Обозреватель семейств.", ex.ToString());
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }

        public void Execute(UIApplication uiApplication) {
            var dockPanelId = new DockablePaneId(FamilyExplorerCommand.DockPanelId);
            if(!DockablePane.PaneIsRegistered(dockPanelId)) {
                uiApplication.RegisterDockablePane(dockPanelId, "Обозреватель семейств", new FamilyExplorerPanelProvider(uiApplication));
            }
        }
    }

    internal class FamilyExplorerPanelProvider : IDockablePaneProvider {
        private readonly UIApplication _uiApplication;
        private FamilyExplorerViewModel _dataContext;

        public FamilyExplorerPanelProvider(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public void SetupDockablePane(DockablePaneProviderData data) {
            var configPath = @"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Ресурсы\99.FamilyExplorer";
            configPath = Path.Combine(configPath, _uiApplication.Application.VersionNumber);
            _dataContext = new FamilyExplorerViewModel(new Models.RevitRepository(_uiApplication), new Models.FamilyRepository(configPath));
            var panel = new FamilyExplorerPanel() { DataContext = _dataContext };
            panel.Loaded += Panel_Loaded;

            data.FrameworkElement = panel;
            data.VisibleByDefault = false;
            data.InitialState.DockPosition = DockPosition.Floating;
        }

        private void Panel_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            _dataContext.LoadSections();
        }
    }
}