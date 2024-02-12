using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models {
    internal class ReportMaker {
        public string MakeMessage(IEnumerable<IResultHandler> resultHandlers) {
            return string.Join("\n", MakeMessages(resultHandlers));
        }

        private IEnumerable<string> MakeMessages(IEnumerable<IResultHandler> resultHandlers) {
            foreach(var code in resultHandlers.GroupBy(r => r.Code)) {
                switch (code.Key)
                {
                    case ResultCode.LintelIsFixedWithoutElement:
                        yield return $"Под данными фиксированными перемычками отсутствует проем: " +
                                     $"{string.Join(", ", code.Select(e => ((ReportResult) e).LintelId))}";
                        break;
                    case ResultCode.LintelGeometricalDisplaced:
                        yield return $"Следующие перемычки смещены от проема: " +
                                     $"{string.Join(", ", code.Select(e => ((ReportResult) e).LintelId))}";
                        break;
                }
            }
        }
    }
}
