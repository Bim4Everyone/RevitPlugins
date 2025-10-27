using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitArchitecturalDocumentation.Models;
internal class SpecHelper {
    private readonly ILocalizationService _localizationService;
    
    public SpecHelper(RevitRepository revitRepository, ScheduleSheetInstance scheduleSheetInstance, TreeReportNode report = null) {

        Report = report;
        Repository = revitRepository;

        SpecSheetInstance = scheduleSheetInstance;
        SpecSheetInstancePoint = scheduleSheetInstance.Point;
        Specification = scheduleSheetInstance.Document.GetElement(scheduleSheetInstance.ScheduleId) as ViewSchedule;
        SpecificationDefinition = Specification.Definition;
        SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
        NameHelper = new ViewNameHelper(Specification);
    }

    public SpecHelper(RevitRepository revitRepository, ViewSchedule viewSchedule, ILocalizationService localizationService,
                      TreeReportNode report = null) {

        Report = report;
        Repository = revitRepository;
        _localizationService = localizationService;

        Specification = viewSchedule;
        SpecificationDefinition = Specification.Definition;
        SpecificationFilters = SpecificationDefinition.GetFilters().ToList();
        NameHelper = new ViewNameHelper(Specification);
    }

    public TreeReportNode Report { get; set; }
    public RevitRepository Repository { get; set; }

    public ScheduleSheetInstance SpecSheetInstance { get; set; }
    public XYZ SpecSheetInstancePoint { get; set; }
    public ViewSchedule Specification { get; set; }
    public ScheduleDefinition SpecificationDefinition { get; set; }
    public List<ScheduleFilter> SpecificationFilters { get; set; }
    public List<string> SpecFilterNames { get; set; }
    public ViewNameHelper NameHelper { get; set; }


    /// <summary>
    /// Метод получает список имен полей спецификации, которые применяются в фильтрах спеки
    /// </summary>
    public List<string> GetFilterNames() {

        SpecFilterNames = SpecificationFilters
            .Select(o => SpecificationDefinition.GetField(o.FieldId))
            .Select(o => o.GetName())
            .Distinct()
            .OrderBy(o => o)
            .ToList();

        return SpecFilterNames;
    }


    /// <summary>
    /// Метод находит в проекте, а если не нашел, то создает спецификацию с указанным именем и задает ей фильтрацию
    /// </summary>
    public SpecHelper GetOrDuplicateNSetSpec(string filterName, int numberOfLevelAsInt) {

        string specName = NameHelper.Prefix
                          + NameHelper.PrefixOfLevelBlock
                          + string.Format(NameHelper.LevelNumberFormat, numberOfLevelAsInt)
                          + NameHelper.SuffixOfLevelBlock
                          + NameHelper.Suffix;

        ViewSchedule newViewSpec = Repository.GetSpecByName(specName);
        SpecHelper newSpecHelper;
        // Если спеку с указанным именем не нашли, то будем создавать дублированием
        if(newViewSpec is null) {
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.SpecWithName")} " +
                $"\"{specName}\" {_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.NotFindLetsCreate")}");
            newViewSpec = Repository.Document.GetElement(Specification.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.SpecCreatedSuccessfully"));
            newViewSpec.Name = specName;
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.NameGiven")} {newViewSpec.Name}");

            newSpecHelper = new SpecHelper(Repository, newViewSpec, _localizationService, Report);
            newSpecHelper.ChangeSpecFilters(filterName, numberOfLevelAsInt);
        } else {
            Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.SpecWithName")} " +
                $"\"{newViewSpec.Name}\" {_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.SpecFinded")}");
            newSpecHelper = new SpecHelper(Repository, newViewSpec, _localizationService, Report);
        }
        newSpecHelper.SpecSheetInstancePoint = SpecSheetInstancePoint;

        return newSpecHelper;
    }


    /// <summary>
    /// Метод по изменению фильтра спецификации с указанным именем на указанное значение с учетом формата предыдущего значения
    /// </summary>
    public void ChangeSpecFilters(string specFilterName, int newFilterValue) {

        // В дальнейшем нужно предусмотреть проверки, что поле фильтрации принимает строки + сеттеры для других типов
        List<ScheduleFilter> newScheduleFilters = [];

        // Перебираем фильтры и записываем каждый, изменяя только тот, что ищем потому что механизм изменения значения конкретного фильтра работал нестабильно
        for(int i = 0; i < SpecificationFilters.Count; i++) {

            ScheduleFilter currentFilter = SpecificationFilters[i];

            ScheduleField scheduleFieldFromFilter = SpecificationDefinition.GetField(currentFilter.FieldId);

            if(scheduleFieldFromFilter.GetName() == specFilterName) {

                string filterOldValue = currentFilter.GetStringValue();
                string format = ViewNameHelper.GetStringFormatOrDefault(filterOldValue);
                string newVal = string.Format(format, newFilterValue);
                currentFilter.SetValue(newVal);

                Report?.AddNodeWithName($"{_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.SetFilterValue")} {currentFilter.GetStringValue()}");
                newScheduleFilters.Add(currentFilter);
            } else {
                newScheduleFilters.Add(currentFilter);
            }
        }

        SpecificationDefinition.SetFilters(newScheduleFilters);
    }


    /// <summary>
    /// Размещает спецификацию на листе
    /// </summary>
    public ScheduleSheetInstance PlaceSpec(SheetHelper sheetHelper) {

        var newScheduleSheetInstance = ScheduleSheetInstance.Create(
                Repository.Document,
                sheetHelper.Sheet.Id,
                Specification.Id,
                SpecSheetInstancePoint);

        SpecSheetInstance = newScheduleSheetInstance;

        if(newScheduleSheetInstance is null) {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.FailedViewportCreation"));
        } else {
            Report?.AddNodeWithName(_localizationService.GetLocalizedString("CopySpecSheetInstanceVM.Report.SuccessViewportCreation"));
        }

        return newScheduleSheetInstance;
    }
}
