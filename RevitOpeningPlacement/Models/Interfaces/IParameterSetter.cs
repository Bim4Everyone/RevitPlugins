using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IParameterSetter {
        void SetParameters(FamilyInstance familyInstance);
    }
}
