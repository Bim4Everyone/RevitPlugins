using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces {
    internal interface IParameterElementViewModel {
        string Name { get; }
        StorageType StorageType { get; }
    }
}
