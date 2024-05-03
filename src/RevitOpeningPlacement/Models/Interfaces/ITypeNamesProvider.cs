using System.Collections.Generic;

namespace RevitOpeningPlacement.Models.Interfaces {
    interface ITypeNamesProvider {
        IEnumerable<string> GetTypeNames();
    }
}
