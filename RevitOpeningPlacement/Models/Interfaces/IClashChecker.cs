using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IClashChecker {
        string Check(ClashModel clashModel);
        string GetMessage();
    }
}
