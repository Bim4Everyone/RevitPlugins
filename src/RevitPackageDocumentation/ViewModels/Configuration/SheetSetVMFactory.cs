using System;
using System.Linq;

using RevitPackageDocumentation.Models;
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
    private readonly RevitRepository _revitRepository;

    public SheetSetVMFactory(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

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
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "123",
                ModuleErrors = "Ошибка PlanView",

                ViewName = data.ViewName ?? string.Empty,
                ViewCount = data.ViewCount,
                ViewFamilyType = _revitRepository.ViewFamilyTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.ViewPlanTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                //ViewportType = 
            },
            ScheduleViewData data => new ScheduleViewVM {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "456",
                ModuleErrors = "Ошибка ScheduleView",

                ReferenceViewName = data.ReferenceViewName ?? string.Empty,
                ViewName = data.ViewName ?? string.Empty,
                ViewCount = data.ViewCount,
                ViewRow = data.ViewRow ?? 1,
            },
            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }
}
