using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitLintelPlacement.ViewModels.Interfaces {
    internal interface ILintelParameterViewModel {
        void SetTo(FamilyInstance lintel, FamilyInstance elementInWall);
    }
}
