using System.Collections.Generic;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitSetCoordParams.Models;
internal class FixedParams {

    private readonly SharedParamsConfig _sharedParamsConfig = SharedParamsConfig.Instance;

    public IEnumerable<(RevitParam RevitParam, string Key, bool IsPair)> GetDefaultParams() {
        return [(_sharedParamsConfig.BuildingWorksBlock, "BlockParam", true),
                (_sharedParamsConfig.BuildingWorksSection, "SectionParam", true),
                (_sharedParamsConfig.BuildingWorksLevel, "FloorParam", true),
                (_sharedParamsConfig.BuildingWorksLevelCurrency, "FloorDEParam", true),
                (_sharedParamsConfig.FixSolution, "BlockingParam", false)];

    }
}
