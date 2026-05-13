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
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                ViewName = vm.ViewName,
                ViewCount = vm.ViewCount,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewTemplateName = vm.ViewTemplate?.Name,
            },
            ScheduleViewVM vm => new ScheduleViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                ViewName = vm.ViewName,
                ViewCount = vm.ViewCount,
                ReferenceViewName = vm.ReferenceViewName,
                ViewRow = vm.ViewRow,
            },
            _ => throw new NotSupportedException()
        };
    }
}
