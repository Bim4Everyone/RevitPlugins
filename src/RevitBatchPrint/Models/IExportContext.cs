using System.Collections.Generic;

using RevitBatchPrint.ViewModels;

namespace RevitBatchPrint.Models;

internal interface IExportContext {
    void Export(IEnumerable<SheetViewModel> sheets);
    bool CanExport(IEnumerable<SheetViewModel> sheets);
}
