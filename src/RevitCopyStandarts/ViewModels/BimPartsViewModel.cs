using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitCopyStandarts.Models;

namespace RevitCopyStandarts.ViewModels;

internal sealed class BimPartsViewModel {
    public BimPartsViewModel(string partName) {
        Name = partName;
    }

    public string Name { get; }
    public ObservableCollection<BimFileViewModel> BimFiles { get; set; }
}
