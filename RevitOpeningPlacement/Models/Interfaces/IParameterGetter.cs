using System.Collections.Generic;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IParameterGetter {
        IEnumerable<ParameterValuePair> GetParamValues();
    }
}
