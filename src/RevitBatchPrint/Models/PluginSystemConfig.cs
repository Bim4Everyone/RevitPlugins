using System;
using System.Collections.Generic;

namespace RevitBatchPrint.Models;

internal static class PluginSystemConfig {
    public static readonly string DefaultPrinterName = "Pdf24";

    public static readonly HashSet<string> PrintParamNames = new(StringComparer.OrdinalIgnoreCase) {
        "Орг.ОбознчТома(Комплекта)", "Орг.КомплектЧертежей", "ADSK_Комплект чертежей"
    };
}
