using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.ClashDetective.Interfaces {
    interface IProviderViewModel {
        bool IsSelected { get; set; }
        string Name { get; }
        IProvider GetProvider(Document doc, Transform transform);
    }
}
