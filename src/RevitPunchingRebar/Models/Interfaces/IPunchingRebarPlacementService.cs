using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitPunchingRebar.Models.Interfaces;
internal interface IPunchingRebarPlacementService {
    void Run(IEnumerable<IPylon> pylons, IFrameParams frameParams, RevitRepository revitRepository);
       
}
