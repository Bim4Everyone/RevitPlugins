using System;
using System.Linq;

using Autodesk.Revit.DB;

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
                ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? 1,
            },

            SectionViewData data => new SectionViewVM {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "789",
                ModuleErrors = "Ошибка SectionView",

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.SectionViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.SectionViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? 1,
            },

            ScheduleViewData data => new ScheduleViewVM {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "456",
                ModuleErrors = "Ошибка ScheduleView",

                ReferenceViewName = data.ReferenceViewName ?? string.Empty,
                ViewName = data.ViewName ?? string.Empty,
                ViewColumn = data.ViewColumn ?? 1,
                ViewCount = data.ViewCount ?? 1,
            },

            TextNoteData data => new TextNoteVM {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "01",
                ModuleErrors = "Ошибка TextNote",

                Text = data.Text ?? string.Empty,
                TextNoteType = _revitRepository.TextNoteTypes.FirstOrDefault(v => v.Name.Equals(data.TextNoteTypeName)),
            },

            TypicalAnnotationData data => CreateTypicalAnnotationVM(data),

            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }

    private TypicalAnnotationVM CreateTypicalAnnotationVM(TypicalAnnotationData data) {

        var annotationFamilies = _revitRepository.GenericAnnotationFamilies;

        var annotationFamily = annotationFamilies?.FirstOrDefault(v => v.Name.Equals(data.AnnotationFamilyName));

        var annotationTypes = annotationFamily
                    ?.GetFamilySymbolIds()
                    ?.Select(id => _revitRepository.Document.GetElement(id) as AnnotationSymbolType)
                    ?.ToList();

        var annotationType = annotationTypes
                    ?.FirstOrDefault(v => v.Name.Equals(data.AnnotationTypeName));

        var t = new TypicalAnnotationVM(_revitRepository) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "02",
            ModuleErrors = "Ошибка TypicalAnnotation",

            AnnotationTypes = annotationTypes,
            AnnotationFamily = annotationFamily,
            AnnotationType = annotationType
        };

        return t;
    }
}
