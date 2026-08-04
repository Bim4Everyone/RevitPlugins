using System.Collections.Generic;

namespace RevitClassifierParameters.Models;

public class WorkGroup : Work{
    public List<WorkGroup> ChildWorkGroups { get; set; } = [];
    public List<Work> ChildWorks { get; set; } = [];
}
