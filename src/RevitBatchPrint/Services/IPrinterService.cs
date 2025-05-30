using System.Collections.Generic;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.Services;

internal interface IPrinterService {
    bool HasPrinterName(string printerName);
    IEnumerable<string> EnumPrinterNames();
    
    IPrinterSettings CreatePrinterSettings(string printerName);
}
