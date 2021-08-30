using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitBatchPrint.Printing {
    internal class PrintManager {
        public string[] GetPrinters() {
            throw new NotImplementedException();
        }

        public bool HasPrinter(string printerName) {
            throw new NotImplementedException();
        }

        public PrinterSettings GetPrinterSettings(string printerName) {
            return new PrinterSettings(printerName);
        }
    }
}
