using System;
using System.Threading;

using Autodesk.Revit.DB;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;
internal class CreateInstanceOptions {

    public TempFamilyDocument TempDoc { get; set; }
    public ViewDrafting ViewDraft { get; set; }
    public int Counter { get; set; }
    public IProgress<int> Progress { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
