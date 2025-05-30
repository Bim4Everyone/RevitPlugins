using System.Collections.Generic;

namespace RevitBatchPrint.Models;

internal interface IRevitPrint {
    void Execute(IReadOnlyCollection<SheetElement> sheets, PrintOptions printOptions);
}
