using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vanara.PInvoke;

namespace RevitBatchPrint.Printing {
    internal class PrintManager {
        public IEnumerable<string> EnumPrinters() {
            return WinSpool.EnumPrinters<WinSpool.PRINTER_INFO_5>().Select(item => item.pPrinterName);
        }

        public bool HasPrinter(string printerName) {
            return EnumPrinters().Any(item => item.Equals(printerName));
        }

        public PrinterSettings GetPrinterSettings(string printerName) {
            return new PrinterSettings(printerName);
        }
    }
}
