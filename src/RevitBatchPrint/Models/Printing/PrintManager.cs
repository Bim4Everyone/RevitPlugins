using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vanara.PInvoke;

namespace RevitBatchPrint.Models.Printing {
    internal class PrintManager {
        public IEnumerable<string> EnumPrinterNames() {
            return WinSpool.EnumPrinters<WinSpool.PRINTER_INFO_5>().Select(item => item.pPrinterName);
        }

        public bool HasPrinterName(string printerName) {
            return EnumPrinterNames().Any(item => item.Equals(printerName));
        }

        public PrinterSettings GetPrinterSettings(string printerName) {
            return new PrinterSettings(printerName);
        }
    }
}