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

namespace RevitMarkPlacement {

    [Transaction(TransactionMode.Manual)]
    public class PlaceAnnotationCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var viewModel = new MainViewModel(
                    new RevitRepository(
                        commandData.Application.Application, 
                        commandData.Application.ActiveUIDocument.Document));
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Расстановка отметок.", ex.ToString());
#else
                TaskDialog.Show("Расстановка отметок.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
