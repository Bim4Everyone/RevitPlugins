using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    interface ICriterion {
        IFilterGenerator FilterGenerator { get; set; }
        IFilterGenerator Generate(Document doc);
    }
}
