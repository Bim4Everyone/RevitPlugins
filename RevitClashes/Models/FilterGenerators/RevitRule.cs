using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterGenerators {
    internal class RevitRule : IRule {
        public FilterRule FilterRule { get; set; }
    }
}
