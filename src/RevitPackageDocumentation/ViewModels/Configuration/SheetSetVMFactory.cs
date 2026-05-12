using System;

using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration;

internal interface ISheetSetVMFactory {
    SheetSetVM CreateSheetSetVM(SheetSetData data);
    SheetVM CreateSheetVM(SheetData data);
    SheetComponentVM CreateComponentVM(SheetComponentData data);
}

internal class SheetSetVMFactory : ISheetSetVMFactory {

    public SheetSetVM CreateSheetSetVM(SheetSetData data) {
        if(data == null)
            throw new ArgumentNullException(nameof(data));

        var sheetSetVM = new SheetSetVM {
            Name = data.Name
        };

        foreach(var sheetData in data.Sheets) {
            var sheetVM = CreateSheetVM(sheetData);
            sheetSetVM.SheetList.Add(sheetVM);
        }

        return sheetSetVM;
    }

    public SheetVM CreateSheetVM(SheetData data) {
        if(data == null)
            throw new ArgumentNullException(nameof(data));

        var sheetVM = new SheetVM {
            Name = data.Name ?? string.Empty
        };

        foreach(var componentData in data.Views) {
            var componentVM = CreateComponentVM(componentData);
            sheetVM.SheetComponents.Add(componentVM);
        }

        return sheetVM;
    }

    public SheetComponentVM CreateComponentVM(SheetComponentData sheetComponentData) {
        return sheetComponentData switch {
            PlanViewData data => new PlanViewVM {
                ModuleName = data.Name ?? string.Empty,
                ViewName = data.PlanName ?? string.Empty,
                ViewCount = data.PlanNumber
            },
            ScheduleViewData data => new ScheduleViewVM {
                ModuleName = data.Name ?? string.Empty,
                ViewName = data.ScheduleName ?? string.Empty,
                ViewCount = data.ScheduleNumber
            },
            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }
}
