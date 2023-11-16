using System.Collections.Generic;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IParametersGetter {
        IEnumerable<ParameterValuePair> GetParamValues();
    }
}
