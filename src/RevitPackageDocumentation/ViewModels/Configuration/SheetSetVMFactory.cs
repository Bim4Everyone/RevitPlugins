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
            ConfigurationName = data.Name
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
            SheetName = data.Name ?? string.Empty
        };

        foreach(var componentData in data.Views) {
            var componentVM = CreateComponentVM(componentData);
            sheetVM.SheetComponents.Add(componentVM);
        }

        return sheetVM;
    }

    public SheetComponentVM CreateComponentVM(SheetComponentData data) {
        return data switch {
            PlanViewData planData => new PlanViewVM {
                ViewName = planData.PlanName ?? string.Empty,
                ViewCount = planData.PlanNumber
            },
            ScheduleViewData scheduleData => new ScheduleViewVM {
                ViewName = scheduleData.ScheduleName ?? string.Empty,
                ViewCount = scheduleData.ScheduleNumber
            },
            _ => throw new NotSupportedException($"Тип '{data?.GetType().Name}' не поддерживается")
        };
    }
}
