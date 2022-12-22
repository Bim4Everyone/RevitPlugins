using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Value;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IValueGetter<T> where T : ParamValue {
        T GetValue();
    }
}
