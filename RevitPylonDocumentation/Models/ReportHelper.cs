using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPylonDocumentation.Models {
    internal class ReportHelper {
        public ReportHelper() {
            _report = new StringBuilder();
        }


        private StringBuilder _report;

        public void AppendLine(string text) {
            _report.AppendLine(text);
        }

        public string GetAsString() {
            return _report.ToString();
        }
    }
}
