using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models.Interfaces {
    internal interface IChecker {
        IResultHandler Check(FamilyInstance lintel, FamilyInstance elementInWall);
    }
}
