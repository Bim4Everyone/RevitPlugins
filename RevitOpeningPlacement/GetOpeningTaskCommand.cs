using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningUnion;
using RevitOpeningPlacement.ViewModels.Navigator;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class GetOpeningTaskCommand : BasePluginCommand {
        public GetOpeningTaskCommand() {
            PluginName = "Навигатор по заданиям";
        }

        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var configurator = new UnionGroupsConfigurator(revitRepository);
            var openingsGroups = GetOpeningsGroups(revitRepository, configurator);
            var viewModel = new OpeningsViewModel(revitRepository, openingsGroups);

            var window = new NavigatorView() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.Show();
        }

        private List<OpeningsGroup> GetOpeningsGroups(RevitRepository revitRepository, UnionGroupsConfigurator configurator) {
            var groups = new List<OpeningsGroup>();
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                IProgress<int> progress;
                CancellationToken ct;
                var maxValue = revitRepository.GetOpenings().Count;
                InitializeProgress(maxValue, pb, out progress, out ct);

                groups.AddRange(configurator.GetGroups(progress, ct));
            }
            return groups;
        }

        private static void InitializeProgress(int maxValue, IProgressDialogService pb, out IProgress<int> progress, out CancellationToken ct) {
            pb.StepValue = 10;
            pb.DisplayTitleFormat = "Идёт расчёт... [{0}\\{1}]";
            progress = pb.CreateProgress();
            pb.MaxValue = maxValue;
            ct = pb.CreateCancellationToken();
            pb.Show();
        }
    }
}