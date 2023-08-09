using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

using Parameter = Autodesk.Revit.DB.Parameter;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewScheduleCreator {
        internal PylonViewScheduleCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        public bool TryCreateRebarSchedule() {

            if(ViewModel.ReferenceRebarSchedule is null || !ViewModel.ReferenceRebarSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { return false; }

            ElementId scheduleId = null;
            ViewSchedule viewSchedule = null;

            try {

                scheduleId = ViewModel.ReferenceRebarSchedule.Duplicate(ViewDuplicateOption.Duplicate);
                viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
                if(viewSchedule != null) {
                    viewSchedule.Name = ViewModel.SchedulesSettings.RebarSchedulePrefix + SheetInfo.PylonKeyName + ViewModel.SchedulesSettings.RebarScheduleSuffix;

                    // Задаем сортировку
                    SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingFirst, ViewModel.SchedulesSettings.RebarScheduleDisp1);
                    SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingSecond, ViewModel.SchedulesSettings.RebarScheduleDisp2);


                    // Задаем фильтры спецификации
                    SetScheduleFilters(viewSchedule);
                }

            } catch(Exception) {

                if(scheduleId != null) {
                    Repository.Document.Delete(scheduleId);
                }
                return false;
            }

            SheetInfo.RebarSchedule.ViewElement = viewSchedule;
            return true;
        }



        public bool TryCreateMaterialSchedule() {

            if(ViewModel.ReferenceMaterialSchedule is null || !ViewModel.ReferenceMaterialSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { return false; }

            ElementId scheduleId = null;
            ViewSchedule viewSchedule = null;

            try {

                scheduleId = ViewModel.ReferenceMaterialSchedule.Duplicate(ViewDuplicateOption.Duplicate);
                viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
                if(viewSchedule is null) { return false; }

                viewSchedule.Name = ViewModel.SchedulesSettings.MaterialSchedulePrefix + SheetInfo.PylonKeyName + ViewModel.SchedulesSettings.MaterialScheduleSuffix;

                // Задаем сортировку
                SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingFirst, ViewModel.SchedulesSettings.MaterialScheduleDisp1);
                SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingSecond, ViewModel.SchedulesSettings.MaterialScheduleDisp2);

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

            if(ViewModel.ReferenceSystemPartsSchedule is null || !ViewModel.ReferenceSystemPartsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { return false; }

            ElementId scheduleId = null;
            ViewSchedule viewSchedule = null;

            try {
                scheduleId = ViewModel.ReferenceSystemPartsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
                viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
                if(viewSchedule is null) { return false; }

                viewSchedule.Name = ViewModel.SchedulesSettings.SystemPartsSchedulePrefix + SheetInfo.PylonKeyName + ViewModel.SchedulesSettings.SystemPartsScheduleSuffix;

                // Задаем сортировку
                SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingFirst, ViewModel.SchedulesSettings.SystemPartsScheduleDisp1);
                SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingSecond, ViewModel.SchedulesSettings.SystemPartsScheduleDisp2);

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


        public bool TryCreateIFCPartsSchedule() {

            if(ViewModel.ReferenceIFCPartsSchedule is null || !ViewModel.ReferenceIFCPartsSchedule.CanViewBeDuplicated(ViewDuplicateOption.Duplicate)) { return false; }

            ElementId scheduleId = null;
            ViewSchedule viewSchedule = null;

            try {
                scheduleId = ViewModel.ReferenceIFCPartsSchedule.Duplicate(ViewDuplicateOption.Duplicate);
                viewSchedule = Repository.Document.GetElement(scheduleId) as ViewSchedule;
                if(viewSchedule is null) { return false; }

                viewSchedule.Name = ViewModel.SchedulesSettings.IFCPartsSchedulePrefix + SheetInfo.PylonKeyName + ViewModel.SchedulesSettings.IFCPartsScheduleSuffix;


                // Задаем сортировку
                SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingFirst, ViewModel.SchedulesSettings.IFCPartsScheduleDisp1);
                SetScheduleDispatcherParameter(viewSchedule, ViewModel.ProjectSettings.DispatcherGroupingSecond, ViewModel.SchedulesSettings.IFCPartsScheduleDisp2);


                // Задаем фильтры спецификации
                SetScheduleFilters(viewSchedule);
            } catch(Exception) {

                if(scheduleId != null) {
                    Repository.Document.Delete(scheduleId);
                }
                return false;
            }

            SheetInfo.IFCPartsSchedule.ViewElement = viewSchedule;
            return true;
        }




        /// <summary>
        /// Задает у спецификации значение параметру диспетчера
        /// </summary>
        public void SetScheduleDispatcherParameter(ViewSchedule viewSchedule, string dispGroupingParam, string hostDispGroupingParam) {

            Parameter ScheduleGroupingParameter = viewSchedule.LookupParameter(dispGroupingParam);
            Parameter HostGroupingParameterValue = SheetInfo.HostElems[0].LookupParameter(hostDispGroupingParam);

            string GroupingParameterValue = string.Empty;

            // Если такого параметра нет, значит просто записываем то, что записал пользователь
            if(HostGroupingParameterValue is null) {
                GroupingParameterValue = hostDispGroupingParam;
            } else {
                // Иначе получаем значение этого параметра из пилона
                GroupingParameterValue = HostGroupingParameterValue.AsValueString();
            }

            if(ScheduleGroupingParameter != null && ScheduleGroupingParameter.StorageType == StorageType.String) {
                ScheduleGroupingParameter.Set(GroupingParameterValue);
            }
        }





        public void SetScheduleFilters(ViewSchedule viewSchedule) {

            ScheduleDefinition scheduleDefinition = viewSchedule.Definition;

            IList<ScheduleFilter> viewScheduleFilters = scheduleDefinition.GetFilters();

            // Идем в обратном порядке, т.к. удаление фильтра происходит по НОМЕРУ фильтра в общем списке в спеке
            // Поэтому, если идти прямо, то номера сдивгаются
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
}
