using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IClashChecker {
        bool Check(ClashModel clashModel);
    }
}
