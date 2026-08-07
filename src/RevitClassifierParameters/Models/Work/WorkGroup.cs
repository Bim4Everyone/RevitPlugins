using System.Collections.Generic;

namespace RevitClassifierParameters.Models.Work;

public class WorkGroup : Work{
    public List<WorkGroup> ChildWorkGroups { get; set; } = [];
    public List<Models.Work.Work> ChildWorks { get; set; } = [];
}
