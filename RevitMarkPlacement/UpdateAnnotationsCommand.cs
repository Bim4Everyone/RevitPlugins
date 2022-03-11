using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.ViewModels;
using RevitMarkPlacement.Views;

namespace RevitMarkPlacement {

    [Transaction(TransactionMode.Manual)]
    public class UpdateAnnotationsCommand : IExternalCommand {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var config = AnnotationsConfig.GetAnnotationsConfig();
                var revitRepository = new RevitRepository(
                        commandData.Application.Application,
                        commandData.Application.ActiveUIDocument.Document);
                var viewModel = new MainViewModel(revitRepository, config) {
                    OnlyUpdate = true,
                };
                viewModel.SelectedMode = viewModel.SelectionModes[0];
                if(!viewModel.CanPlaceAnnotation()) {
                    var view = new ReportView() { DataContext = viewModel.InfoElementsViewModel };
                    view.ShowDialog();
                } else {
                    viewModel.PlaceAnnotationCommand.Execute(null);
                }
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Обновление отметок.", ex.ToString());
#else
                TaskDialog.Show("Обновление отметок.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
