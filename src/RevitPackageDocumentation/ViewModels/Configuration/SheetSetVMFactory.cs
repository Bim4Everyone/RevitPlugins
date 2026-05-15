using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

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
    private readonly ILocalizationService _localizationService;

    public SheetSetVMFactory(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    public SheetSetVM CreateSheetSetVM(SheetSetData data) {
        if(data == null)
            throw new ArgumentNullException(nameof(data));

        var sheetSetVM = new SheetSetVM(_revitRepository) {
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

        var titleBlockFamily = _revitRepository.TitleBlockFamilies?.FirstOrDefault(v => v.Name.Equals(data.TitleBlockFamilyName));

        var titleBlockTypes = titleBlockFamily
            ?.GetFamilySymbolIds()
            ?.Select(id => _revitRepository.Document.GetElement(id) as FamilySymbol)
            ?.ToList();

        var titleBlockType = titleBlockTypes?.FirstOrDefault(v => v.Name.Equals(data.TitleBlockTypeName));

        var sheetVM = new SheetVM(_revitRepository) {
            Name = data.Name ?? string.Empty,
            SheetSize = data.SheetSize ?? string.Empty,
            SheetCoefficient = data.SheetCoefficient ?? string.Empty,

            TitleBlockFamily = titleBlockFamily,
            TitleBlockTypes = titleBlockTypes,
            TitleBlockType = titleBlockType,
        };

        foreach(var componentData in data.Views) {
            var componentVM = CreateComponentVM(componentData);
            sheetVM.SheetComponents.Add(componentVM);
        }

        return sheetVM;
    }

    public SheetComponentVM CreateComponentVM(SheetComponentData sheetComponentData) {
        return sheetComponentData switch {
            StructuralPlanViewData data => new StructuralPlanViewVM(_localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "123",
                ModuleErrors = string.Empty,

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? "1",
            },

            StructuralCalloutViewData data => new StructuralCalloutViewVM(_localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "05",
                ModuleErrors = string.Empty,

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? "1",
            },

            SectionViewData data => new SectionViewVM(_localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "789",
                ModuleErrors = string.Empty,

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.SectionViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.SectionViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? "1",
            },

            ScheduleViewData data => new ScheduleViewVM(_localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "456",
                ModuleErrors = string.Empty,

                ReferenceViewName = data.ReferenceViewName ?? string.Empty,
                ViewName = data.ViewName ?? string.Empty,
                ViewColumn = data.ViewColumn ?? "1",
                ViewCount = data.ViewCount ?? "1",
            },

            TextNoteData data => new TextNoteVM(_localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "01",
                ModuleErrors = string.Empty,

                Text = data.Text ?? string.Empty,
                TextNoteType = _revitRepository.TextNoteTypes.FirstOrDefault(v => v.Name.Equals(data.TextNoteTypeName)),
            },

            TypicalAnnotationData data => CreateTypicalAnnotationVM(data),

            LegendViewData data => new LegendViewVM(_localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "03",
                ModuleErrors = string.Empty,

                LegendView = _revitRepository.LegendsInProject.FirstOrDefault(v => v.Name.Equals(data.ViewName)),
            },

            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }

    private TypicalAnnotationVM CreateTypicalAnnotationVM(TypicalAnnotationData data) {

        var annotationFamily = _revitRepository.GenericAnnotationFamilies?.FirstOrDefault(v => v.Name.Equals(data.AnnotationFamilyName));

        var annotationTypes = annotationFamily
                    ?.GetFamilySymbolIds()
                    ?.Select(id => _revitRepository.Document.GetElement(id) as AnnotationSymbolType)
                    ?.ToList();

        var annotationType = annotationTypes
                    ?.FirstOrDefault(v => v.Name.Equals(data.AnnotationTypeName));

        var t = new TypicalAnnotationVM(_revitRepository, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "02",
            ModuleErrors = string.Empty,

            AnnotationTypes = annotationTypes,
            AnnotationFamily = annotationFamily,
            AnnotationType = annotationType
        };

        return t;
    }
}
