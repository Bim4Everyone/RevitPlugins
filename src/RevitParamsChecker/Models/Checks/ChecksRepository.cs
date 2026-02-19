using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitParamsChecker.Models.Checks;

internal class ChecksRepository {
    private readonly ChecksConfig _checksConfig;

    public ChecksRepository(ChecksConfig checksConfig) {
        _checksConfig = checksConfig ?? throw new ArgumentNullException(nameof(checksConfig));
    }

    public void SetChecks(ICollection<Check> checks) {
        _checksConfig.Checks = checks?.ToArray() ?? throw new ArgumentNullException(nameof(checks));
        _checksConfig.SaveProjectConfig();
    }

    public ICollection<Check> GetChecks() {
        return new ReadOnlyCollection<Check>(_checksConfig.Checks);
    }
}
