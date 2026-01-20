using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewScheduleCreator {
    internal PylonViewScheduleCreator(CreationSettings settings, Document document, PylonSheetInfo pylonSheetInfo) {
        ProjectSettings = settings.ProjectSettings;
        SchedulesSettings = settings.SchedulesSettings;
        ScheduleFiltersSettings = settings.ScheduleFiltersSettings;
        Doc = document;
        SheetInfo = pylonSheetInfo;
    }

    internal UserProjectSettings ProjectSettings { get; set; }
    internal UserSchedulesSettings SchedulesSettings { get; set; }
    internal UserScheduleFiltersSettings ScheduleFiltersSettings { get; set; }
    internal Document Doc { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    public bool TryCreateSkeletonSchedule() {
        if(SchedulesSettings.SelectedSkeletonSchedule is null
            || !SchedulesSettings.SelectedSkeletonSchedule
                    .CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = SchedulesSettings.SelectedSkeletonSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Doc.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule != null) {
                viewSchedule.Name =
                    SchedulesSettings.SkeletonSchedulePrefix
                    + SheetInfo.PylonKeyName
                    + SchedulesSettings.SkeletonScheduleSuffix;

                // Задаем сортировку
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ProjectSettings.DispatcherGroupingFirst,
                    SchedulesSettings.SkeletonScheduleDisp1);
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ProjectSettings.DispatcherGroupingSecond,
                    SchedulesSettings.SkeletonScheduleDisp2);

                // Задаем фильтры спецификации
                SetScheduleFilters(viewSchedule);
            }
        } catch(Exception) {
            if(scheduleId != null) {
                Doc.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.SkeletonSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateSkeletonByElemsSchedule() {
        if(SchedulesSettings.SelectedSkeletonByElemsSchedule is null
            || !SchedulesSettings.SelectedSkeletonByElemsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = SchedulesSettings.SelectedSkeletonByElemsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Doc.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule != null) {
                viewSchedule.Name =
                    SchedulesSettings.SkeletonByElemsSchedulePrefix
                    + SheetInfo.PylonKeyName
                    + SchedulesSettings.SkeletonByElemsScheduleSuffix;

                // Задаем сортировку
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ProjectSettings.DispatcherGroupingFirst,
                    SchedulesSettings.SkeletonByElemsScheduleDisp1);
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ProjectSettings.DispatcherGroupingSecond,
                    SchedulesSettings.SkeletonByElemsScheduleDisp2);

                // Задаем фильтры спецификации
                SetScheduleFilters(viewSchedule);
            }
        } catch(Exception) {
            if(scheduleId != null) {
                Doc.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.SkeletonByElemsSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateMaterialSchedule() {
        if(SchedulesSettings.SelectedMaterialSchedule is null
            || !SchedulesSettings.SelectedMaterialSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = SchedulesSettings.SelectedMaterialSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Doc.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule is null) { return false; }

            viewSchedule.Name = SchedulesSettings.MaterialSchedulePrefix
                                + SheetInfo.PylonKeyName
                                + SchedulesSettings.MaterialScheduleSuffix;

            // Задаем сортировку
            SetScheduleDispatcherParameter(viewSchedule,
                                           ProjectSettings.DispatcherGroupingFirst,
                                           SchedulesSettings.MaterialScheduleDisp1);
            SetScheduleDispatcherParameter(viewSchedule,
                                           ProjectSettings.DispatcherGroupingSecond,
                                           SchedulesSettings.MaterialScheduleDisp2);

            // Задаем фильтры спецификации
            SetScheduleFilters(viewSchedule);
        } catch(Exception) {
            if(scheduleId != null) {
                Doc.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.MaterialSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateSystemPartsSchedule() {
        if(SchedulesSettings.SelectedSystemPartsSchedule is null
            || !SchedulesSettings.SelectedSystemPartsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = SchedulesSettings.SelectedSystemPartsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Doc.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule is null) { return false; }

            viewSchedule.Name = SchedulesSettings.SystemPartsSchedulePrefix
                                + SheetInfo.PylonKeyName
                                + SchedulesSettings.SystemPartsScheduleSuffix;

            // Задаем сортировку
            SetScheduleDispatcherParameter(viewSchedule,
                                           ProjectSettings.DispatcherGroupingFirst,
                                           SchedulesSettings.SystemPartsScheduleDisp1);
            SetScheduleDispatcherParameter(viewSchedule,
                                           ProjectSettings.DispatcherGroupingSecond,
                                           SchedulesSettings.SystemPartsScheduleDisp2);

            // Задаем фильтры спецификации
            SetScheduleFilters(viewSchedule);
        } catch(Exception) {
            if(scheduleId != null) {
                Doc.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.SystemPartsSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateIfcPartsSchedule() {
        if(SchedulesSettings.SelectedIfcPartsSchedule is null
            || !SchedulesSettings.SelectedIfcPartsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = SchedulesSettings.SelectedIfcPartsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Doc.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule is null) { return false; }

            viewSchedule.Name = SchedulesSettings.IfcPartsSchedulePrefix
                                + SheetInfo.PylonKeyName
                                + SchedulesSettings.IfcPartsScheduleSuffix;

            // Задаем сортировку
            SetScheduleDispatcherParameter(viewSchedule,
                                           ProjectSettings.DispatcherGroupingFirst,
                                           SchedulesSettings.IfcPartsScheduleDisp1);
            SetScheduleDispatcherParameter(viewSchedule,
                                           ProjectSettings.DispatcherGroupingSecond,
                                           SchedulesSettings.IfcPartsScheduleDisp2);

            // Задаем фильтры спецификации
            SetScheduleFilters(viewSchedule);
        } catch(Exception) {

            if(scheduleId != null) {
                Doc.Delete(scheduleId);
            }
            return false;
        }
        SheetInfo.IfcPartsSchedule.ViewElement = viewSchedule;
        return true;
    }


    /// <summary>
    /// Задает у спецификации значение параметру диспетчера
    /// </summary>
    public void SetScheduleDispatcherParameter(ViewSchedule viewSchedule, string dispGroupingParam,
                                               string hostDispGroupingParam) {
        var scheduleGroupingParameter = viewSchedule.LookupParameter(dispGroupingParam);
        var hostGroupingParameterValue = SheetInfo.HostElems[0].LookupParameter(hostDispGroupingParam);

        string groupingParameterValue = string.Empty;

        // Если такого параметра нет, значит просто записываем то, что записал пользователь
        if(hostGroupingParameterValue is null) {
            groupingParameterValue = hostDispGroupingParam;
        } else {
            // Иначе получаем значение этого параметра из пилона
            groupingParameterValue = hostGroupingParameterValue.AsValueString();
        }

        if(scheduleGroupingParameter != null && scheduleGroupingParameter.StorageType == StorageType.String) {
            scheduleGroupingParameter.Set(groupingParameterValue);
        }
    }


    public void SetScheduleFilters(ViewSchedule viewSchedule) {
        ScheduleDefinition scheduleDefinition = viewSchedule.Definition;
        IList<ScheduleFilter> viewScheduleFilters = scheduleDefinition.GetFilters();

        // Идем в обратном порядке, т.к. удаление фильтра происходит по НОМЕРУ фильтра в общем списке в спеке
        // Поэтому, если идти прямо, то номера сдвигаются
        for(int i = viewScheduleFilters.Count - 1; i >= 0; i--) {
            ScheduleFilter currentFilter = viewScheduleFilters[i];

            // Получаем поле спеки из фильтра
            ScheduleField scheduleFieldFromFilter = scheduleDefinition.GetField(currentFilter.FieldId);

            // Определяем есть ли параметр фильтра в списке нужных
            ScheduleFilterParamHelper filterParam = ScheduleFiltersSettings.ParamsForScheduleFilters
                .FirstOrDefault(item => item.ParamNameInSchedule.Equals(scheduleFieldFromFilter.GetName()));

            // Если его нет в списке нужных - удаляем
            if(filterParam is null) {
                scheduleDefinition.RemoveFilter(i);
            } else {
                // Если параметр есть в списке нужных, то пытаемся найти соответствующий параметр в пилоне
                Parameter paramInHost = SheetInfo.HostElems[0].LookupParameter(filterParam.ParamNameInHost);
                if(paramInHost is null) {
                    continue;
                }

                // Если он есть в пилоне, то задаем значение как в соответствующем параметре пилона
                if(currentFilter.IsDoubleValue) {
                    currentFilter.SetValue(paramInHost.AsDouble());
                } else if(currentFilter.IsIntegerValue) {
                    currentFilter.SetValue(paramInHost.AsInteger());
                } else if(currentFilter.IsElementIdValue) {
                    currentFilter.SetValue(paramInHost.AsElementId());
                } else {
                    currentFilter.SetValue(paramInHost.AsValueString());
                }
                scheduleDefinition.SetFilter(i, currentFilter);
            }
        }
    }
}
