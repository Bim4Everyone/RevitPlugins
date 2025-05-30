using System.Collections.Generic;
using System.Linq;

using RevitBatchPrint.Models;

using Vanara.PInvoke;

namespace RevitBatchPrint.Services;

internal sealed class PrinterService : IPrinterService {
    private IReadOnlyCollection<string> _printerNames;

    public void Load() {
        _printerNames = WinSpool.EnumPrinters<WinSpool.PRINTER_INFO_5>()
            .Select(item => item.pPrinterName)
            .ToArray();
    }

    public IEnumerable<string> EnumPrinterNames() {
        return _printerNames;
    }

    public bool HasPrinterName(string printerName) {
        return _printerNames
            .Any(item => item.Equals(printerName));
    }

    public IPrinterSettings CreatePrinterSettings(string printerName) {
        return new PrinterSettings(printerName).Load();
    }
}
