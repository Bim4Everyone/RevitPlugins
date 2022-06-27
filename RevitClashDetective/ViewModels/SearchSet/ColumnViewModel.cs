using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class ColumnViewModel : BaseViewModel {
        public string FieldName { get; set; }
        public string Header { get; set; }
        public StorageType FieldType { get; set; }
    }
}
