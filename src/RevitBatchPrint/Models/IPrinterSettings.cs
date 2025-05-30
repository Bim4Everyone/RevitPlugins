using System.Collections.Generic;
using System.Drawing;

namespace RevitBatchPrint.Models;

internal interface IPrinterSettings {
    bool HasFormat(string formatName);
    IEnumerable<string> EnumFormatNames();
        
    void RemoveFormat(string formatName);
    void AddFormat(string formatName, Size formatSize);
}
