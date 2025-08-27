using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFinishing.ViewModels.Notices;

internal interface IWarningItemViewModel {
    string ElementIdInfo { get; }
    string ElementName { get; }
    string CategoryInfo { get; }
    string PhaseName { get; }
    string LevelName { get; }
}
