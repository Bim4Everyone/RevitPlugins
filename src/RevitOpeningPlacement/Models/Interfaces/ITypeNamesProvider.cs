using System.Collections.Generic;

namespace RevitOpeningPlacement.Models.Interfaces;
internal interface ITypeNamesProvider {
    IEnumerable<string> GetTypeNames();
}
