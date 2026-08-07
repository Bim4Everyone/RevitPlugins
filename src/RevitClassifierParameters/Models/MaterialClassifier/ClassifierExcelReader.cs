using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using dosymep.SimpleServices;

using Microsoft.Office.Interop.Excel;

using RevitClassifierParameters.Models.Work;

namespace RevitClassifierParameters.Models.MaterialClassifier;

public class ClassifierExcelReader {
    private const int _firstDataRow = 3;
    
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;

    public ClassifierExcelReader(
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService) {
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
    }
    
    public List<WorkGroup> Read(string path) {
        Application excel = null;
        Workbook workbook = null;
        Worksheet worksheet = null;
        Range range = null;

        try {
            excel = new Application {
                Visible = false,
                DisplayAlerts = false,
                ScreenUpdating = false
            };
            workbook = excel.Workbooks.Open(path, ReadOnly: true);
            worksheet = (Worksheet) workbook.Worksheets[1];

            int lastRow = GetLastRow(worksheet);
            if(lastRow < _firstDataRow) {
                _messageBoxService.Show("Лист классификатора пуст или не содержит данных.");
                return null;
            }

            range = worksheet.Range[
                worksheet.Cells[_firstDataRow, 1],
                worksheet.Cells[lastRow, 3]];

            if(range.Value2 is not object[,] values) {
                _messageBoxService.Show("Лист классификатора пуст или не содержит данных.");
                return null;
            }

            return BuildClassifier(values);
        } finally {
            workbook?.Close(false);
            excel?.Quit();

            Release(range);
            Release(worksheet);
            Release(workbook);
            Release(excel);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private List<WorkGroup> BuildClassifier(object[,] rows) {
        var result = new List<WorkGroup>();
        var groups = new Dictionary<string, WorkGroup>();

        int rowCount = rows.GetLength(0);

        for(int row = 1; row <= rowCount; row++) {
            string code = GetString(rows[row, 1]);
            if(string.IsNullOrWhiteSpace(code))
                continue;

            string name = GetString(rows[row, 2]);
            string unit = GetString(rows[row, 3]);

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
        Dictionary<string, WorkGroup> groups,
        string code,
        string name) {
        var group = new WorkGroup {
            Code = code,
            Name = name
        };

        groups.Add(code, group);

        var parent = FindParentGroup(groups, code);
        if(parent != null) {
            group.ParentWorkGroup = parent;
            parent.ChildWorkGroups.Add(group);
        } else {
            roots.Add(group);
        }
    }

    private void AddWork(
        Dictionary<string, WorkGroup> groups,
        string code,
        string name,
        string unit) {
        var parent = FindParentGroup(groups, code);
        if(parent == null)
            return;

        parent.ChildWorks.Add(new Work.Work {
            Code = code,
            Name = name,
            Unit = unit,
            ParentWorkGroup = parent
        });
    }

    private int GetLastRow(Worksheet worksheet) {
        var lastCell = worksheet.Cells.Find(
            "*",
            Type.Missing,
            Type.Missing,
            Type.Missing,
            XlSearchOrder.xlByRows,
            XlSearchDirection.xlPrevious,
            false);
        try {
            return lastCell?.Row ?? 1;
        } finally {
            Release(lastCell);
        }
    }

    private string GetString(object value) {
        return value?.ToString()?.Trim();
    }

    private static WorkGroup FindParentGroup(
        Dictionary<string, WorkGroup> groups,
        string code) {
        string current = code;
        int separatorIndex;

        while((separatorIndex = current.LastIndexOf('.')) >= 0) {
            current = current.Substring(0, separatorIndex);
            if(groups.TryGetValue(current, out var parent))
                return parent;
        }
        return null;
    }

    private void Release(object comObject) {
        if(comObject != null && Marshal.IsComObject(comObject))
            Marshal.ReleaseComObject(comObject);
    }
}
