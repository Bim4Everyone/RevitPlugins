using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitClassifierParameters.Models.Work;

namespace RevitClassifierParameters.Models.MaterialClassifier;

internal class ClassifierExcelReader : ExcelReaderBase<List<WorkGroup>> {
    public ClassifierExcelReader(
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService)
        : base(localizationService, messageBoxService) {
    }

    protected override int FirstDataRow => 3;

    protected override int ColumnCount => 3;

    protected override string EmptySheetMessageKey => "Reader.ClassifierSheetEmpty";

    protected override List<WorkGroup> BuildResult(object[,] rows) {
        var result = new List<WorkGroup>();
        var groups = new List<WorkGroup>();

        int rowCount = rows.GetLength(0);

        for(int row = 0; row < rowCount; row++) {
            string code = GetString(rows[row, 0]);
            if(string.IsNullOrWhiteSpace(code)) {
                continue;
            }

            string name = GetString(rows[row, 1]);
            string unit = GetString(rows[row, 2]);

            if(string.IsNullOrWhiteSpace(unit)) {
                AddGroup(result, groups, code, name);
            } else {
                AddWork(groups, code, name, unit);
            }
        }
        return result;
    }

    private void AddGroup(
        List<WorkGroup> roots,
        List<WorkGroup> groups,
        string code,
        string name) {
        var group = new WorkGroup {
            Code = code,
            Name = name
        };

        groups.Add(group);

        var parent = FindParentGroup(groups, code);
        if(parent != null) {
            group.ParentWorkGroup = parent;
            parent.ChildWorkGroups.Add(group);
        } else {
            roots.Add(group);
        }
    }

    private void AddWork(
        List<WorkGroup> groups,
        string code,
        string name,
        string unit) {
        var parent = FindParentGroup(groups, code);

        parent?.ChildWorks.Add(new WorkItem {
            Code = code,
            Name = name,
            Unit = unit,
            ParentWorkGroup = parent
        });
    }

    private static WorkGroup FindParentGroup(
        List<WorkGroup> groups,
        string code) {
        string current = code;
        int separatorIndex;

        while((separatorIndex = current.LastIndexOf('.')) >= 0) {
            current = current.Substring(0, separatorIndex);
            var parent = groups.FirstOrDefault(g => g.Code == current);
            if(parent != null) {
                return parent;
            }
        }
        return null;
    }
}
