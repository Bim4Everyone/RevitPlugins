using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningViewModel : BaseViewModel {
        public int Id { get; set; }
        public string Level { get; set; }
        public string TypeName { get; set; }
        public string FamilyName { get; set; }
    }
}
