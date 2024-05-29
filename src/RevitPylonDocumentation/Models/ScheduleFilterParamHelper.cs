using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPylonDocumentation.Models
{
    class ScheduleFilterParamHelper
    {
        public ScheduleFilterParamHelper(string paramNameInSchedule, string paramNameInHost) {
            ParamNameInSchedule = paramNameInSchedule;
            ParamNameInHost = paramNameInHost;
        }

        public bool IsCheck { get; set; } = false;
        public string ParamNameInSchedule { get; set; }
        public string ParamNameInHost { get; set; }
    }
}
