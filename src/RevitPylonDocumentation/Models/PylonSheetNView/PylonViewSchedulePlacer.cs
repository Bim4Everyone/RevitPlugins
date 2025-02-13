using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewSchedulePlacer {
        internal PylonViewSchedulePlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        internal bool PlaceRebarSchedule() {
            // Проверяем вдруг спека не создалась
            if(SheetInfo.RebarSchedule.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.RebarSchedule.ViewportName =
                    ViewModel.SchedulesSettings.RebarSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.RebarScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.RebarSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации арматуры пилона
            XYZ newCenter = new XYZ(
                (-SheetInfo.RebarSchedule.ViewportHalfWidth * 2) - 0.0095,
                SheetInfo.TitleBlockHeight - 0.032,
                0);
            (SheetInfo.RebarSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.RebarSchedule.ViewportCenter = newCenter;
            return true;
        }

        internal bool PlaceSkeletonSchedule() {
            // Проверяем вдруг спека не создалась
            if(SheetInfo.SkeletonSchedule.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.SkeletonSchedule.ViewportName =
                    ViewModel.SchedulesSettings.SkeletonSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.SkeletonScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.SkeletonSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации арматуры пилона
            XYZ newCenter = new XYZ(
                (-SheetInfo.SkeletonSchedule.ViewportHalfWidth * 2) - 0.0095,
                SheetInfo.TitleBlockHeight - 0.032,
                0);
            (SheetInfo.SkeletonSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.SkeletonSchedule.ViewportCenter = newCenter;
            return true;
        }

        internal bool PlaceSkeletonByElemsSchedule() {
            // Проверяем вдруг спека не создалась
            if(SheetInfo.SkeletonByElemsSchedule.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.SkeletonByElemsSchedule.ViewportName =
                    ViewModel.SchedulesSettings.SkeletonByElemsSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.SkeletonByElemsScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.SkeletonByElemsSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации арматуры пилона
            XYZ newCenter = new XYZ(
                (-SheetInfo.SkeletonByElemsSchedule.ViewportHalfWidth * 2) - 0.0095,
                SheetInfo.TitleBlockHeight - 0.032,
                0);
            (SheetInfo.SkeletonByElemsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.SkeletonByElemsSchedule.ViewportCenter = newCenter;
            return true;
        }


        internal bool PlaceMaterialSchedule() {
            // Проверяем вдруг спека не создалась
            if(SheetInfo.MaterialSchedule.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.MaterialSchedule.ViewportName =
                    ViewModel.SchedulesSettings.MaterialSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.MaterialScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.MaterialSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации материалов пилона
            XYZ newCenter = new XYZ(
                    (-SheetInfo.MaterialSchedule.ViewportHalfWidth * 2) - 0.0095,
                    SheetInfo.TitleBlockHeight - (SheetInfo.RebarSchedule.ViewportHalfHeight * 2) - 0.02505,
                    0);

            if(SheetInfo.RebarSchedule.ViewportElement is null) {
                newCenter = new XYZ(
                    (-SheetInfo.MaterialSchedule.ViewportHalfWidth * 2) - 0.0095,
                    -0.1,
                    0);
            }

            (SheetInfo.MaterialSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;
            SheetInfo.MaterialSchedule.ViewportCenter = newCenter;
            return true;
        }


        internal bool PlaceSystemPartsSchedule() {
            // Проверяем вдруг спека не создалась
            if(SheetInfo.SystemPartsSchedule.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.SystemPartsSchedule.ViewportName =
                    ViewModel.SchedulesSettings.SystemPartsSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.SystemPartsScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.SystemPartsSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации системных деталей пилона
            XYZ newCenter = new XYZ(
                    0,
                    (SheetInfo.TitleBlockHeight / 2) + (SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2),
                    0);

            if(SheetInfo.IfcPartsSchedule.ViewportElement is null) {
                newCenter = new XYZ(
                    -UnitUtilsHelper.ConvertToInternalValue(2.9) - (SheetInfo.SystemPartsSchedule.ViewportHalfWidth * 2),
                    (SheetInfo.TitleBlockHeight / 2) + (SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2),
                    0);
            }

            (SheetInfo.SystemPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;
            SheetInfo.SystemPartsSchedule.ViewportCenter = newCenter;
            return true;
        }


        internal bool PlaceIfcPartsSchedule() {
            // Проверяем вдруг спека не создалась
            if(SheetInfo.IfcPartsSchedule.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.IfcPartsSchedule.ViewportName =
                    ViewModel.SchedulesSettings.IfcPartsSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.IfcPartsScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.IfcPartsSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации системных деталей пилона
            XYZ newCenter = new XYZ(
                    0,
                    (SheetInfo.TitleBlockHeight / 2) + (SheetInfo.IfcPartsSchedule.ViewportHalfHeight * 2),
                    0);

            if(SheetInfo.SystemPartsSchedule.ViewportElement is null) {
                newCenter = new XYZ(
                    -UnitUtilsHelper.ConvertToInternalValue(2.9) - (SheetInfo.IfcPartsSchedule.ViewportHalfWidth * 2),
                    (SheetInfo.TitleBlockHeight / 2) + (SheetInfo.IfcPartsSchedule.ViewportHalfHeight * 2),
                    0);
            }

            (SheetInfo.IfcPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;
            SheetInfo.IfcPartsSchedule.ViewportCenter = newCenter;
            return true;
        }


        public bool PlaceScheduleViewport(ViewSheet viewSheet, PylonView pylonView) {
            Document doc = Repository.Document;

            ScheduleSheetInstance scheduleSheetInstance = null;
            // Размещаем спеку пилона на листе
            try {
                scheduleSheetInstance = ScheduleSheetInstance.Create(doc, viewSheet.Id, pylonView.ViewElement.Id, new XYZ(0, 0, 0));
            } catch(Exception) {
                return false;
            }
            pylonView.ViewportElement = scheduleSheetInstance;

            // Получение габаритов видового экрана спецификации
            GetScheduleViewportInfo(viewSheet, pylonView, scheduleSheetInstance);
            return true;
        }


        /// <summary>
        /// Получает и запоминает точку вставки спецификации и ее габариты
        /// </summary>
        public void GetScheduleViewportInfo(ViewSheet viewSheet, PylonView pylonView, ScheduleSheetInstance viewport) {
            XYZ viewportCenter = viewport.Point;

            // Точка вставки спеки в верхнем левый угол спеки
            BoundingBoxXYZ boundingBoxXYZ = viewport.get_BoundingBox(viewSheet);
            double viewportHalfWidth = (boundingBoxXYZ.Max.X - viewportCenter.X) / 2;
            double viewportHalfHeight = (viewportCenter.Y - boundingBoxXYZ.Min.Y) / 2;

            // Запись центра и габаритов видового экрана спецификации
            pylonView.ViewportCenter = viewportCenter;
            pylonView.ViewportHalfWidth = viewportHalfWidth;
            pylonView.ViewportHalfHeight = viewportHalfHeight;
        }
    }
}
