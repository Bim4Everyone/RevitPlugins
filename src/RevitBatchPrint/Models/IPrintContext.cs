using System.Collections.Generic;

using RevitBatchPrint.ViewModels;

namespace RevitBatchPrint.Models;

internal interface IPrintContext {
    void ExecutePrintExport(IEnumerable<SheetViewModel> sheets);
    bool CanExecutePrintExport(IEnumerable<SheetViewModel> sheets);
}
