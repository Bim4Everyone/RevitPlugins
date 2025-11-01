using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitArchitecturalDocumentation.Models;
internal class SheetHelper {
    private readonly ILocalizationService _localizationService;

    public SheetHelper(RevitRepository revitRepository, ILocalizationService localizationService, 
                       TreeReportNode report = null) {
        Repository = revitRepository;
        _localizationService = localizationService;
        Report = report;
        NameHelper = new ViewNameHelper(null, localizationService);
    }
    public SheetHelper(RevitRepository revitRepository, ViewSheet sheet, ILocalizationService localizationService,
                       TreeReportNode report = null) {
        Repository = revitRepository;
        Sheet = sheet;
        _localizationService = localizationService;
        Report = report;
        NameHelper = new ViewNameHelper(sheet, localizationService);
    }

        
    public RevitRepository Repository { get; set; }
    public ViewSheet Sheet { get; set; }


    public ViewNameHelper NameHelper { get; set; }
    public TreeReportNode Report { get; set; }


    /// <summary>
    /// Находит в проекте, а если не нашел, то создает лист с указанным именем.
    /// </summary>
    public ViewSheet GetOrCreateSheet(string newSheetName, FamilySymbol titleBlockType, string widthParamName = "", string heightParamName = "", int width = 0, int height = 0) {

        var newSheet = Repository.GetSheetByName(newSheetName);
        Sheet = newSheet;
        if(newSheet is null) {
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.SheetWithName")} " +
                $"\"{newSheetName}\" {_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.NotFindLetsCreate")}");
            try {
                CreateSheet(newSheetName, titleBlockType);

                if(widthParamName.Length != 0 && heightParamName.Length != 0) {
                    SetUpSheetDimensions(widthParamName, heightParamName, width, height);
                }
            } catch(Exception) {
                Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.FailedSheetCreation"));
            }
        } else {
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.SheetWithName")}" +
                $" \"{newSheetName}\" {_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.SuccessFoundInProject")}");

            NameHelper = new ViewNameHelper(newSheet, _localizationService);
            NameHelper.AnalyzeNGetLevelNumber();
        }

        return newSheet;
    }



    /// <summary>
    /// Создает лист, задает ему имя и тип рамки
    /// </summary>
    public ViewSheet CreateSheet(string newSheetName, FamilySymbol titleBlockType) {

        if(newSheetName.Length == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.InvalidNameForTask"));

            return null;
        }
        if(titleBlockType is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.InvalidTitleBlockType"));
            return null;
        }

        ViewSheet newSheet = null;
        try {
            newSheet = ViewSheet.Create(Repository.Document, titleBlockType.Id);
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.SheetCreatedSuccessfully"));
            newSheet.Name = newSheetName;
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.NameGiven")} {newSheet.Name}");
        } catch(Exception) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.FailedSheetCreation"));
        }

        Sheet = newSheet;
        NameHelper = new ViewNameHelper(Sheet, _localizationService);
        NameHelper.AnalyzeNGetLevelNumber();
        return newSheet;
    }


    /// <summary>
    /// Задает габариты рамки листа через указанные параметры
    /// </summary>
    public void SetUpSheetDimensions(string widthParamName, string heightParamName, int width, int height) {

        if(widthParamName.Length == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.InvalidSheetWidthParamName"));
            return;
        }
        if(heightParamName.Length == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.InvalidSheetHeightParamName"));
            return;
        }
        if(width == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.InvalidSheetWidthParamValue"));
            return;
        }
        if(height == 0) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.InvalidSheetHeightParamValue"));
            return;
        }

        try {
            // Ищем рамку на листе
            FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, Sheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock is null) {
                Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.TitleBlockNotFound"));
                return;

            } else {
                var widthParam = titleBlock.LookupParameter(widthParamName);
                var heightParam = titleBlock.LookupParameter(heightParamName);

                if(widthParam != null && heightParam != null) {
                    widthParam.Set(UnitUtilsHelper.ConvertToInternalValue(width));
                    heightParam.Set(UnitUtilsHelper.ConvertToInternalValue(height));
                    Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.SettedTitleBlockDimension")} {width}х{height}");
                } else {
                    Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.TitleBlockHasNotDimensionParam"));
                    return;
                }
                Repository.Document.Regenerate();
            }

        } catch(Exception) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CreatingARDocsVM.Report.Sheet.FailedSheetCreation"));
        }
    }


    /// <summary>
    /// Проверяет есть ли спецификация с указанным именем на листе
    /// </summary>
    public bool HasSpecWithName(string specName) {

        var doc = Sheet.Document;
        ScheduleSheetInstance schedule = Sheet.GetDependentElements(new ElementClassFilter(typeof(ScheduleSheetInstance)))
            .Select(id => doc.GetElement(id) as ScheduleSheetInstance)
            .FirstOrDefault(v => v.Name == specName);

        return schedule is not null;
    }
}
