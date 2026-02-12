using System;
using System.Collections.Generic;
using System.Threading;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface IIntersectProcessor {
    IReadOnlyCollection<WarningElement> Run(IProgress<int> progress = null, CancellationToken ct = default);
    IEnumerable<RevitElement> RevitElements { get; }
}
