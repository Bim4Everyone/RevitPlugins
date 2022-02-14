using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.ViewModels.ReportViewModel {
    internal class InfoElement {
        public string Message { get; set; }
        public TypeInfo TypeInfo { get; set; }
        public InfoElement FormatMessage(params string[] args) {
            return new InfoElement() {
                TypeInfo = TypeInfo,
                Message = string.Format(Message, args)
            };
        }
    }
}
