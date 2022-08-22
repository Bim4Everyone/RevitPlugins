using System.Collections.Generic;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IParametersGetter {
        IEnumerable<ParameterValuePair> GetParamValues();
    }
}
