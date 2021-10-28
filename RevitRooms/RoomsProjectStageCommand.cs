using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

namespace RevitRooms {
    [Transaction(TransactionMode.Manual)]
    public class RoomsProjectStageCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {

            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Квартирография Стадии П.", ex.ToString());
#else
                TaskDialog.Show("Квартирография Стадии П.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}
