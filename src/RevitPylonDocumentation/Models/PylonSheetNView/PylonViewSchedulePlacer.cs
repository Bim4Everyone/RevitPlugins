using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewSchedulePlacer {
    // Смещение по горизонтали в футах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameRightOffset = UnitUtilsHelper.ConvertToInternalValue(2.883);

    // Смещение по вертикали в футах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(9.7536);

    // Смещения по вертикали в футах, для размещаемых спецификаций
    // требуемое, для их корректного взаимного размещения на листе (в случае наличия маленькой шапки спецификации)
    private readonly double _scheduleTopOffsetSmall = UnitUtilsHelper.ConvertToInternalValue(2.117);
    // требуемое, для их корректного взаимного размещения на листе (в случае наличия большой шапки спецификации)
    private readonly double _scheduleTopOffsetBig = UnitUtilsHelper.ConvertToInternalValue(4);//0.01;

    // Стандартная позиция спецификаций по вертикали в футах, в случае, если размещение пошло не корректно и
    // референсные объекты для размещения на листе не найдены
    private readonly double _defaultSchedulePositionY = UnitUtilsHelper.ConvertToInternalValue(-30.48);

    // Смещение по горизонтали в футах, для размещаемых ведомостей деталей, для корректного размещения на листе
    private readonly double _schedulePartsRightOffset = -UnitUtilsHelper.ConvertToInternalValue(2.9);


    internal PylonViewSchedulePlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    internal bool PlaceSkeletonSchedule() {
        // Проверяем вдруг спека не создалась
        if(SheetInfo.SkeletonSchedule.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
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
        var newCenter = new XYZ(
            -SheetInfo.SkeletonSchedule.ViewportHalfWidth * 2 - _titleBlockFrameRightOffset,
            SheetInfo.TitleBlockHeight - _titleBlockFrameTopOffset,
            0);
        (SheetInfo.SkeletonSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

        SheetInfo.SkeletonSchedule.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceMaterialSchedule() {
        // Проверяем вдруг спека не создалась
        if(SheetInfo.MaterialSchedule.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
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
        var newCenter = new XYZ(
                -SheetInfo.MaterialSchedule.ViewportHalfWidth * 2 - _titleBlockFrameRightOffset,
                _defaultSchedulePositionY,
                0);

        if(SheetInfo.SkeletonSchedule.ViewportElement != null) {
            newCenter = new XYZ(
                -SheetInfo.MaterialSchedule.ViewportHalfWidth * 2 - _titleBlockFrameRightOffset,
                SheetInfo.SkeletonSchedule.ViewportCenter.Y - SheetInfo.SkeletonSchedule.ViewportHalfHeight * 2
                + _scheduleTopOffsetSmall,
                0);
        }
        (SheetInfo.MaterialSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;
        SheetInfo.MaterialSchedule.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceSkeletonByElemsSchedule() {
        // Проверяем вдруг спека не создалась
        if(SheetInfo.SkeletonByElemsSchedule.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.SkeletonByElemsSchedule.ViewportName =
                ViewModel.SchedulesSettings.SkeletonByElemsSchedulePrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.SchedulesSettings.SkeletonByElemsScheduleSuffix;
        }

        // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
        if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.SkeletonByElemsSchedule)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки спецификации материалов пилона
        var newCenter = new XYZ(
                -SheetInfo.SkeletonByElemsSchedule.ViewportHalfWidth * 2 - _titleBlockFrameRightOffset,
                _defaultSchedulePositionY,
                0);

        if(SheetInfo.MaterialSchedule.ViewportElement != null) {
            newCenter = new XYZ(
                -SheetInfo.SkeletonByElemsSchedule.ViewportHalfWidth * 2 - _titleBlockFrameRightOffset,
                SheetInfo.MaterialSchedule.ViewportCenter.Y - SheetInfo.MaterialSchedule.ViewportHalfHeight * 2 - _scheduleTopOffsetBig,
                0);
        }
        (SheetInfo.SkeletonByElemsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

        SheetInfo.SkeletonByElemsSchedule.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceSystemPartsSchedule() {
        // Проверяем вдруг спека не создалась
        if(SheetInfo.SystemPartsSchedule.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
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
        var newCenter = new XYZ(
                _schedulePartsRightOffset - SheetInfo.SystemPartsSchedule.ViewportHalfWidth * 2,
                SheetInfo.TitleBlockHeight / 2 + SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2,
                0);

        if(SheetInfo.SkeletonByElemsSchedule.ViewportElement != null) {
            newCenter = new XYZ(
                _schedulePartsRightOffset - SheetInfo.SystemPartsSchedule.ViewportHalfWidth * 2,
                SheetInfo.SkeletonByElemsSchedule.ViewportCenter.Y - SheetInfo.SkeletonByElemsSchedule.ViewportHalfHeight * 2 - _scheduleTopOffsetBig,
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
            // Заполняем данные для задания
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
        var newCenter = new XYZ(
                _schedulePartsRightOffset - SheetInfo.IfcPartsSchedule.ViewportHalfWidth * 2,
                SheetInfo.TitleBlockHeight / 2 + SheetInfo.IfcPartsSchedule.ViewportHalfHeight * 2,
                0);

        if(SheetInfo.SkeletonByElemsSchedule.ViewportElement != null) {
            newCenter = new XYZ(
                _schedulePartsRightOffset - SheetInfo.IfcPartsSchedule.ViewportHalfWidth * 2,
                SheetInfo.SkeletonByElemsSchedule.ViewportCenter.Y - SheetInfo.SkeletonByElemsSchedule.ViewportHalfHeight * 2 - _scheduleTopOffsetBig,
                0);
        }
        (SheetInfo.IfcPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;
        SheetInfo.IfcPartsSchedule.ViewportCenter = newCenter;
        return true;
    }


    public bool PlaceScheduleViewport(ViewSheet viewSheet, PylonView pylonView) {
        var doc = Repository.Document;

        ScheduleSheetInstance scheduleSheetInstance;
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
        var viewportCenter = viewport.Point;

        // Точка вставки спеки в верхнем левый угол спеки
        var boundingBoxXYZ = viewport.get_BoundingBox(viewSheet);
        double viewportHalfWidth = (boundingBoxXYZ.Max.X - viewportCenter.X) / 2;
        double viewportHalfHeight = (viewportCenter.Y - boundingBoxXYZ.Min.Y) / 2;

        // Запись центра и габаритов видового экрана спецификации
        pylonView.ViewportCenter = viewportCenter;
        pylonView.ViewportHalfWidth = viewportHalfWidth;
        pylonView.ViewportHalfHeight = viewportHalfHeight;
    }
}
