using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models.Interfaces {
    internal interface ILintelsProvider {
        ICollection<FamilyInstance> GetLintels();
    }
}
