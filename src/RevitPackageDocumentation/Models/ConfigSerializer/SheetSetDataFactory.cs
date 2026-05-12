using System;
using System.Linq;

using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

// Только для VM → DTO
internal interface ISheetSetDataMapper {
    SheetSetData ToData(SheetSetVM vm);
    SheetData ToData(SheetVM vm);
    SheetComponentData ToData(SheetComponentVM vm);
}

internal class SheetSetDataMapper : ISheetSetDataMapper {
    public SheetSetData ToData(SheetSetVM vm) {
        if(vm == null) return new SheetSetData();
        
        return new SheetSetData {
            Name = vm.ConfigurationName,
            Sheets = vm.SheetList?.Select(ToData).ToList()
        };
    }
    
    public SheetData ToData(SheetVM vm) {
        if(vm == null) return new SheetData();
        
        return new SheetData {
            Name = vm.SheetName,
            Views = vm.SheetComponents?.Select(ToData).ToList()
        };
    }
    
    public SheetComponentData ToData(SheetComponentVM vm) {
        return vm switch {
            PlanViewVM plan => new PlanViewData { 
                PlanName = plan.ViewName, 
                PlanNumber = plan.ViewCount 
            },
            ScheduleViewVM schedule => new ScheduleViewData { 
                ScheduleName = schedule.ViewName, 
                ScheduleNumber = schedule.ViewCount 
            },
            _ => throw new NotSupportedException()
        };
    }
}
