using System.Collections.Generic;

namespace RevitClassifierParameters.Models.Work;

internal class WorkGroup : WorkItem {
    public List<WorkGroup> ChildWorkGroups { get; set; } = [];
    public List<WorkItem> ChildWorks { get; set; } = [];
}
