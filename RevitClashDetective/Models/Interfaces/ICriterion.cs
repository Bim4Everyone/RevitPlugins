using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitClashDetective.Models.Interfaces {
    interface ICriterion {
        IFilterGenerator FilterGenerator { get; set; }
        IFilterGenerator Generate();
    }
}
