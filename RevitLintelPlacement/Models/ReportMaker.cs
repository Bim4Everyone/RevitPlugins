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
            foreach(var type in resultHandlers.GroupBy(r => r.GetType())) {
                switch (type.First())
                {
                    case LintelIsFixedWithoutElement _:
                        yield return $"Под данными фиксированными перемычками отсутствует проем: " +
                                     $"{string.Join(", ", type.Select(e => ((LintelIsFixedWithoutElement) e).LintelId))}";
                        break;
                    case LintelGeometricalDisplaced _:
                        yield return $"Следующие перемычки смещены от проема: " +
                                     $"{string.Join(", ", type.Select(e => ((LintelGeometricalDisplaced) e).LintelId))}";
                        break;
                }
            }
        }
    }
}
