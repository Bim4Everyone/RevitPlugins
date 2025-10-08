using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewScheduleCreator {
    internal PylonViewScheduleCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    public bool TryCreateSkeletonSchedule() {
        if(ViewModel.ReferenceSkeletonSchedule is null 
            || !ViewModel.ReferenceSkeletonSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = ViewModel.ReferenceSkeletonSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule != null) {
                viewSchedule.Name =
                    ViewModel.SchedulesSettings.SkeletonSchedulePrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.SchedulesSettings.SkeletonScheduleSuffix;

                // Задаем сортировку
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ViewModel.ProjectSettings.DispatcherGroupingFirst,
                    ViewModel.SchedulesSettings.SkeletonScheduleDisp1);
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ViewModel.ProjectSettings.DispatcherGroupingSecond,
                    ViewModel.SchedulesSettings.SkeletonScheduleDisp2);

                // Задаем фильтры спецификации
                SetScheduleFilters(viewSchedule);
            }
        } catch(Exception) {
            if(scheduleId != null) {
                Repository.Document.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.SkeletonSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateSkeletonByElemsSchedule() {
        if(ViewModel.ReferenceSkeletonByElemsSchedule is null
            || !ViewModel.ReferenceSkeletonByElemsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) {
            return false;
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = ViewModel.ReferenceSkeletonByElemsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule != null) {
                viewSchedule.Name =
                    ViewModel.SchedulesSettings.SkeletonByElemsSchedulePrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.SchedulesSettings.SkeletonByElemsScheduleSuffix;

                // Задаем сортировку
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ViewModel.ProjectSettings.DispatcherGroupingFirst,
                    ViewModel.SchedulesSettings.SkeletonByElemsScheduleDisp1);
                SetScheduleDispatcherParameter(
                    viewSchedule,
                    ViewModel.ProjectSettings.DispatcherGroupingSecond,
                    ViewModel.SchedulesSettings.SkeletonByElemsScheduleDisp2);

                // Задаем фильтры спецификации
                SetScheduleFilters(viewSchedule);
            }
        } catch(Exception) {
            if(scheduleId != null) {
                Repository.Document.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.SkeletonByElemsSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateMaterialSchedule() {
        if(ViewModel.ReferenceMaterialSchedule is null 
            || !ViewModel.ReferenceMaterialSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { 
            return false; 
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = ViewModel.ReferenceMaterialSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule is null) { return false; }

            viewSchedule.Name = ViewModel.SchedulesSettings.MaterialSchedulePrefix 
                                + SheetInfo.PylonKeyName 
                                + ViewModel.SchedulesSettings.MaterialScheduleSuffix;

            // Задаем сортировку
            SetScheduleDispatcherParameter(viewSchedule, 
                                           ViewModel.ProjectSettings.DispatcherGroupingFirst, 
                                           ViewModel.SchedulesSettings.MaterialScheduleDisp1);
            SetScheduleDispatcherParameter(viewSchedule, 
                                           ViewModel.ProjectSettings.DispatcherGroupingSecond, 
                                           ViewModel.SchedulesSettings.MaterialScheduleDisp2);

            // Задаем фильтры спецификации
            SetScheduleFilters(viewSchedule);
        } catch(Exception) {
            if(scheduleId != null) {
                Repository.Document.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.MaterialSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateSystemPartsSchedule() {
        if(ViewModel.ReferenceSystemPartsSchedule is null 
            || !ViewModel.ReferenceSystemPartsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { 
            return false; 
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = ViewModel.ReferenceSystemPartsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule is null) { return false; }

            viewSchedule.Name = ViewModel.SchedulesSettings.SystemPartsSchedulePrefix 
                                + SheetInfo.PylonKeyName 
                                + ViewModel.SchedulesSettings.SystemPartsScheduleSuffix;

            // Задаем сортировку
            SetScheduleDispatcherParameter(viewSchedule, 
                                           ViewModel.ProjectSettings.DispatcherGroupingFirst, 
                                           ViewModel.SchedulesSettings.SystemPartsScheduleDisp1);
            SetScheduleDispatcherParameter(viewSchedule, 
                                           ViewModel.ProjectSettings.DispatcherGroupingSecond, 
                                           ViewModel.SchedulesSettings.SystemPartsScheduleDisp2);

            // Задаем фильтры спецификации
            SetScheduleFilters(viewSchedule);
        } catch(Exception) {
            if(scheduleId != null) {
                Repository.Document.Delete(scheduleId);
            }
            return false;
        }

        SheetInfo.SystemPartsSchedule.ViewElement = viewSchedule;
        return true;
    }


    public bool TryCreateIfcPartsSchedule() {
        if(ViewModel.ReferenceIfcPartsSchedule is null 
            || !ViewModel.ReferenceIfcPartsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { 
            return false; 
        }

        ElementId scheduleId = null;
        ViewSchedule viewSchedule;
        try {
            scheduleId = ViewModel.ReferenceIfcPartsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
            viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
            if(viewSchedule is null) { return false; }

            viewSchedule.Name = ViewModel.SchedulesSettings.IfcPartsSchedulePrefix 
                                + SheetInfo.PylonKeyName 
                                + ViewModel.SchedulesSettings.IfcPartsScheduleSuffix;

            // Задаем сортировку
            SetScheduleDispatcherParameter(viewSchedule, 
                                           ViewModel.ProjectSettings.DispatcherGroupingFirst, 
                                           ViewModel.SchedulesSettings.IfcPartsScheduleDisp1);
            SetScheduleDispatcherParameter(viewSchedule, 
                                           ViewModel.ProjectSettings.DispatcherGroupingSecond, 
                                           ViewModel.SchedulesSettings.IfcPartsScheduleDisp2);

            // Задаем фильтры спецификации
            SetScheduleFilters(viewSchedule);
        } catch(Exception) {

            if(scheduleId != null) {
                Repository.Document.Delete(scheduleId);
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
            ScheduleFilterParamHelper filterParam = ViewModel.SchedulesSettings.ParamsForScheduleFilters
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
