using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bim4Everyone.RevitFiltration;

namespace RevitMarkAllDocuments.Models.Filtration;

internal class FilterOptions : IOptions {
    public double Tolerance { get; set; }
}
