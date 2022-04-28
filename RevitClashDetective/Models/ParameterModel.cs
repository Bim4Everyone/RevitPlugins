using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal class ParameterModel {
        public string Name { get; set; }
        public StorageType StoragetType { get; set; }
    }
}
