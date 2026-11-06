using System.Collections.Generic;

namespace RevitPunchingRebar.Models.Interfaces;
internal interface IPunchingRebarPlacementService {
    void Run(IEnumerable<IPylon> pylons, IFrameParams frameParams, RevitRepository revitRepository);
}
