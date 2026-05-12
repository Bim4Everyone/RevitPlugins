using System;
using System.Linq;

using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

internal interface ISheetSetDataFactory {
    SheetSetData CreateSheetSetData(SheetSetVM vm);
    SheetData CreateSheetData(SheetVM vm);
    SheetComponentData CreateComponentData(SheetComponentVM vm);
}

internal class SheetSetDataFactory : ISheetSetDataFactory {
    public SheetSetData CreateSheetSetData(SheetSetVM vm) {
        if(vm == null)
            return new SheetSetData();

        return new SheetSetData {
            Name = vm.Name,
            Sheets = vm.SheetList?.Select(CreateSheetData).ToList()
        };
    }

    public SheetData CreateSheetData(SheetVM vm) {
        if(vm == null)
            return new SheetData();

        return new SheetData {
            Name = vm.Name,
            Views = vm.SheetComponents?.Select(CreateComponentData).ToList()
        };
    }

    public SheetComponentData CreateComponentData(SheetComponentVM sheetComponentVM) {
        return sheetComponentVM switch {
            PlanViewVM vm => new PlanViewData {
                Name = vm.ModuleName,
                PlanName = vm.ViewName,
                PlanNumber = vm.ViewCount
            },
            ScheduleViewVM vm => new ScheduleViewData {
                Name = vm.ModuleName,
                ScheduleName = vm.ViewName,
                ScheduleNumber = vm.ViewCount
            },
            _ => throw new NotSupportedException()
        };
    }
}
