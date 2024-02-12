using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.Models.Interfaces {
    internal interface IResultHandler {
        ResultCode Code { get; set; }
        void Handle();
    }
}
