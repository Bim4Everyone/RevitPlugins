using System.Collections.Generic;

using RevitBatchPrint.ViewModels;

namespace RevitBatchPrint.Models;

internal interface IPrintContext {
    void Print(IEnumerable<SheetViewModel> sheets);
    bool CanPrint(IEnumerable<SheetViewModel> sheets);
}
