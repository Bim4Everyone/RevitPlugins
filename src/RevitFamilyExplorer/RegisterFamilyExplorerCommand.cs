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
using dosymep.Bim4Everyone;

using RevitFamilyExplorer.ViewModels;
using RevitFamilyExplorer.Views;

namespace RevitFamilyExplorer {
    [Transaction(TransactionMode.Manual)]
    public class RegisterFamilyExplorerCommand  : BasePluginCommand {
        public RegisterFamilyExplorerCommand() {
            PluginName = "Обозреватель семейств";
        }
        
        protected override void Execute(UIApplication uiApplication) {
            var dockPanelId = new DockablePaneId(FamilyExplorerCommand.DockPanelId);
            if(!DockablePane.PaneIsRegistered(dockPanelId)) {
                uiApplication.RegisterDockablePane(dockPanelId, "Обозреватель семейств", new FamilyExplorerPanelProvider(uiApplication));
            }
        }

        public void RegisterPanel(UIApplication uiApplication) {
            Execute(uiApplication);
        }
    }

    internal class FamilyExplorerPanelProvider : IDockablePaneProvider {
        private readonly UIApplication _uiApplication;
        private FamilyExplorerViewModel _dataContext;

        public FamilyExplorerPanelProvider(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public void SetupDockablePane(DockablePaneProviderData data) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var configPath = @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\99.FamilyExplorer";
                configPath = Path.Combine(configPath, _uiApplication.Application.VersionNumber);
                _dataContext = new FamilyExplorerViewModel(new Models.RevitRepository(_uiApplication), new Models.FamilyRepository(configPath));
                var panel = new FamilyExplorerPanel() { DataContext = _dataContext };
                panel.Loaded += Panel_Loaded;

                data.FrameworkElement = panel;
                data.VisibleByDefault = false;
                data.InitialState.DockPosition = DockPosition.Floating;
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }
        }

        private void Panel_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                _dataContext.LoadSections();
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }
        }
    }
}
