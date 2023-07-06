using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitPylonDocumentation.ViewModels;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models {
    public class PylonViewPlacer {
        internal PylonViewPlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }



        // Метод для размещения основного вида пилона.
        // Позиционирование - левый верхний угол листа
        internal bool PlaceGeneralViewport() {

            // Проверям вдруг вид не создался
            if(SheetInfo.GeneralView.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.GeneralView.ViewScale = 25;
                SheetInfo.GeneralView.ViewportTypeName = "Заголовок на листе";
                SheetInfo.GeneralView.ViewportNumber = "100";
                SheetInfo.GeneralView.ViewportName = 
                    ViewModel.GENERAL_VIEW_PREFIX 
                    + SheetInfo.PylonKeyName 
                    + ViewModel.GENERAL_VIEW_SUFFIX;
            }

            // Передаем основной вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralView)) {
                return false;
            }

            // Скрытие категории Разразы, Оси и Уровни
            SetSectionCategoryVisibility(SheetInfo.GeneralView.ViewElement, new List<BuiltInCategory> { 
                BuiltInCategory.OST_Sections,
                BuiltInCategory.OST_Grids,
                BuiltInCategory.OST_Levels
            }, false);


            // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
            if(SheetInfo.GeneralView.ViewportHalfHeight * 2 > SheetInfo.TitleBlockHeight) {
                SheetInfo.SetTitleBlockSize(ViewModel._revitRepository.Document, 2, 1);
            }

            double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth + 0.065;

            // Рассчитываем и задаем корректную точку вставки основного вида пилон, если есть еще и перпендикулярный
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                newCenterX = newCenterX - SheetInfo.GeneralView.ViewportHalfWidth - SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth - 0.065;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    SheetInfo.TitleBlockHeight - SheetInfo.GeneralView.ViewportHalfHeight - 0.016,
                    0);

            (SheetInfo.GeneralView.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.GeneralView.ViewportCenter = newCenter;

            // Включение видимости категории Разразы
            SetSectionCategoryVisibility(SheetInfo.GeneralView.ViewElement, new List<BuiltInCategory> {
                BuiltInCategory.OST_Sections
            }, true);


            return true;
        }


        // Метод для размещения основного вида пилона.
        // Позиционирование - левый верхний угол листа
        internal bool PlaceGeneralPerpendicularViewport() {

            // Проверям вдруг вид не создался
            if(SheetInfo.GeneralViewPerpendicular.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.GeneralViewPerpendicular.ViewScale = 25;
                SheetInfo.GeneralViewPerpendicular.ViewportTypeName = "Заголовок на листе";
                SheetInfo.GeneralViewPerpendicular.ViewportNumber = "101";
                SheetInfo.GeneralViewPerpendicular.ViewportName =
                    ViewModel.GENERAL_VIEW_PERPENDICULAR_PREFIX
                    + SheetInfo.PylonKeyName
                    + ViewModel.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            }

            // Передаем основной перпендикулярный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralViewPerpendicular)) {
                return false;
            }

            // Скрытие категории Разразы
            SheetInfo.GeneralViewPerpendicular.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sections).Id, true);
            SheetInfo.GeneralViewPerpendicular.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Grids).Id, true);
            SheetInfo.GeneralViewPerpendicular.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Levels).Id, true);

            // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона
            double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + 0.065;

            // Рассчитываем и задаем корректную точку вставки основного вида пилона
            if(SheetInfo.GeneralView.ViewportElement != null) {
                newCenterX = newCenterX - SheetInfo.GeneralView.ViewportHalfWidth - SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth - 0.065;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    SheetInfo.TitleBlockHeight - SheetInfo.GeneralViewPerpendicular.ViewportHalfHeight - 0.016,
                    0);

            (SheetInfo.GeneralViewPerpendicular.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.GeneralViewPerpendicular.ViewportCenter = newCenter;

            // Включение видимости категории Разразы
            SheetInfo.GeneralViewPerpendicular.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sections).Id, false);

            return true;
        }


        internal bool PlaceTransverseFirstViewPorts() {

            // Проверям вдруг вид не создался
            if(SheetInfo.TransverseViewFirst.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.TransverseViewFirst.ViewScale = 20;
                SheetInfo.TransverseViewFirst.ViewportTypeName = "Сечение_Номер вида";
                SheetInfo.TransverseViewFirst.ViewportNumber = "1";
                SheetInfo.TransverseViewFirst.ViewportName = "";
            }
            
            // Передаем первый поперечный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewFirst)) {
                return false;
            }

            // Скрытие категории Разразы
            SheetInfo.TransverseViewFirst.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sections).Id, true);
            SheetInfo.TransverseViewFirst.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Grids).Id, true);
            SheetInfo.TransverseViewFirst.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Levels).Id, true);

            // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
            double GeneralViewX = 0;
            double GeneralViewPerpendicularX = 0;
            double newCenterX = 0;
            double newCenterY = 0;



            // Если видовой экран основного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralView.ViewportElement != null) {
                GeneralViewX = SheetInfo.GeneralView.ViewportCenter.X;
            }

            // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                GeneralViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
            }


            // Определяем координату Х первого поперечного вида пилона
            if(SheetInfo.GeneralView.OnSheet && SheetInfo.GeneralViewPerpendicular.OnSheet) {
                if(GeneralViewX > GeneralViewPerpendicularX) {
                    newCenterX = GeneralViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
                } else {
                    newCenterX = GeneralViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
                }
            } else if(SheetInfo.GeneralView.OnSheet && !SheetInfo.GeneralViewPerpendicular.OnSheet) {
                newCenterX = GeneralViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
            } else if(!SheetInfo.GeneralView.OnSheet && SheetInfo.GeneralViewPerpendicular.OnSheet) {
                newCenterX = GeneralViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
            } else {
                // Когда обоих видовых экранов нет на листе
                newCenterX = SheetInfo.TitleBlockWidth / 2;
            }

            if(SheetInfo.TransverseViewSecond.ViewportElement != null || SheetInfo.TransverseViewThird.ViewportElement != null) {
                newCenterY = UnitUtilsHelper.ConvertToInternalValue(- 25);
            } else {
                newCenterY = 0.016 + SheetInfo.TransverseViewFirst.ViewportHalfHeight;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    newCenterY,
                    0);


            (SheetInfo.TransverseViewFirst.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.TransverseViewFirst.ViewportCenter = newCenter;

            return true;
        }

        internal bool PlaceTransverseSecondViewPorts() {
            //double coordinateX = 0;
            //double coordinateY = 0;

            // Проверям вдруг вид не создался
            if(SheetInfo.TransverseViewSecond.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.TransverseViewSecond.ViewScale = 20;
                SheetInfo.TransverseViewSecond.ViewportTypeName = "Сечение_Номер вида";
                SheetInfo.TransverseViewSecond.ViewportNumber = "2";
                SheetInfo.TransverseViewSecond.ViewportName = "";
            }

            // Передаем первый поперечный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewSecond)) {
                return false;
            }

            // Скрытие категории Разразы
            SheetInfo.TransverseViewSecond.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sections).Id, true);
            SheetInfo.TransverseViewSecond.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Grids).Id, true);
            SheetInfo.TransverseViewSecond.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Levels).Id, true);

            // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
            double GeneralViewX = 0;
            double GeneralViewPerpendicularX = 0;
            double newCenterX = 0;
            double newCenterY = 0;

            // Если видовой экран основного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralView.ViewportElement != null) {
                GeneralViewX = SheetInfo.GeneralView.ViewportCenter.X;
            }

            // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                GeneralViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
            }


            // Определяем координату Х первого поперечного вида пилона
            if(SheetInfo.GeneralView.OnSheet && SheetInfo.GeneralViewPerpendicular.OnSheet) {
                if(GeneralViewX > GeneralViewPerpendicularX) {
                    newCenterX = GeneralViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
                } else {
                    newCenterX = GeneralViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
                }
            } else if(SheetInfo.GeneralView.OnSheet && !SheetInfo.GeneralViewPerpendicular.OnSheet) {
                newCenterX = GeneralViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
            } else if(!SheetInfo.GeneralView.OnSheet && SheetInfo.GeneralViewPerpendicular.OnSheet) {
                newCenterX = GeneralViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
            } else {
                // Когда обоих видовых экранов нет на листе
                newCenterX = SheetInfo.TitleBlockWidth / 2;
            }

            if(SheetInfo.TransverseViewFirst.ViewportElement is null || SheetInfo.TransverseViewThird.ViewportElement != null) {
                newCenterY = UnitUtilsHelper.ConvertToInternalValue(- 50);
            } else {
                newCenterY = SheetInfo.TransverseViewFirst.ViewportCenter.Y
                    + SheetInfo.TransverseViewFirst.ViewportHalfHeight 
                    + SheetInfo.TransverseViewSecond.ViewportHalfHeight;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    newCenterY,
                    0);


            (SheetInfo.TransverseViewSecond.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.TransverseViewSecond.ViewportCenter = newCenter;

            return true;
        }

        internal bool PlaceTransverseThirdViewPorts() {
            //double coordinateX = 0;
            //double coordinateY = 0;

            // Проверям вдруг вид не создался
            if(SheetInfo.TransverseViewThird.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.TransverseViewThird.ViewScale = 20;
                SheetInfo.TransverseViewThird.ViewportTypeName = "Сечение_Номер вида";
                SheetInfo.TransverseViewThird.ViewportNumber = "3";
                SheetInfo.TransverseViewThird.ViewportName = "";
            }

            // Передаем первый поперечный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewThird)) {
                return false;
            }

            // Скрытие категории Разразы
            SheetInfo.TransverseViewThird.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sections).Id, true);
            SheetInfo.TransverseViewThird.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Grids).Id, true);
            SheetInfo.TransverseViewThird.ViewElement.SetCategoryHidden(Repository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Levels).Id, true);

            // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
            double GeneralViewX = 0;
            double GeneralViewPerpendicularX = 0;
            double newCenterX = 0;
            double newCenterY = 0;

            // Если видовой экран основного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralView.ViewportElement != null) {
                GeneralViewX = SheetInfo.GeneralView.ViewportCenter.X;
            }

            // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                GeneralViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
            }


            // Определяем координату Х первого поперечного вида пилона
            if(SheetInfo.GeneralView.OnSheet && SheetInfo.GeneralViewPerpendicular.OnSheet) {
                if(GeneralViewX > GeneralViewPerpendicularX) {
                    newCenterX = GeneralViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
                } else {
                    newCenterX = GeneralViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
                }
            } else if(SheetInfo.GeneralView.OnSheet && !SheetInfo.GeneralViewPerpendicular.OnSheet) {
                newCenterX = GeneralViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
            } else if(!SheetInfo.GeneralView.OnSheet && SheetInfo.GeneralViewPerpendicular.OnSheet) {
                newCenterX = GeneralViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
            } else {
                // Когда обоих видовых экранов нет на листе
                newCenterX = SheetInfo.TitleBlockWidth / 2;
            }

            if(SheetInfo.TransverseViewSecond.ViewportElement is null) {
                newCenterY = UnitUtilsHelper.ConvertToInternalValue(-75);

            } else {
                newCenterY = SheetInfo.TransverseViewSecond.ViewportCenter.Y
                    + SheetInfo.TransverseViewSecond.ViewportHalfHeight
                    + SheetInfo.TransverseViewThird.ViewportHalfHeight;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    newCenterY,
                    0);


            (SheetInfo.TransverseViewThird.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.TransverseViewThird.ViewportCenter = newCenter;

            return true;
        }


        internal bool PlaceRebarSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.RebarSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.RebarSchedule.ViewportName =
                    ViewModel.REBAR_SCHEDULE_PREFIX
                        + SheetInfo.PylonKeyName
                        + ViewModel.REBAR_SCHEDULE_SUFFIX;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.RebarSchedule)) {
                return false;
            }


            // Рассчитываем и задаем корректную точку вставки спецификации арматуры пилона
            XYZ newCenter = new XYZ(
                -SheetInfo.RebarSchedule.ViewportHalfWidth * 2 - 0.0095,
                SheetInfo.TitleBlockHeight - 0.032,
                0);
            (SheetInfo.RebarSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.RebarSchedule.ViewportCenter = newCenter;

            return true;
        }

        internal bool PlaceMaterialSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.MaterialSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.MaterialSchedule.ViewportName =
                    ViewModel.MATERIAL_SCHEDULE_PREFIX
                        + SheetInfo.PylonKeyName
                        + ViewModel.MATERIAL_SCHEDULE_SUFFIX;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.MaterialSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации материалов пилона
            XYZ newCenter = new XYZ(
                    -SheetInfo.MaterialSchedule.ViewportHalfWidth * 2 - 0.0095,
                    SheetInfo.TitleBlockHeight - SheetInfo.RebarSchedule.ViewportHalfHeight * 2 - 0.02505,
                    0);


            if(SheetInfo.RebarSchedule.ViewportElement is null) {
                newCenter = new XYZ(
                    -SheetInfo.MaterialSchedule.ViewportHalfWidth * 2 - 0.0095,
                    -0.1,
                    0);
            }
            
            
            (SheetInfo.MaterialSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.MaterialSchedule.ViewportCenter = newCenter;

            return true;
        }


        internal bool PlaceSystemPartsSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.SystemPartsSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.SystemPartsSchedule.ViewportName =
                    ViewModel.SYSTEM_PARTS_SCHEDULE_PREFIX
                        + SheetInfo.PylonKeyName
                        + ViewModel.SYSTEM_PARTS_SCHEDULE_SUFFIX;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.SystemPartsSchedule)) {
                return false;
            }







            // Рассчитываем и задаем корректную точку вставки спецификации системных деталей пилона
            XYZ newCenter = new XYZ(
                    0,
                    UnitUtilsHelper.ConvertToInternalValue(120) + SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2,
                    0);

            if(SheetInfo.IFCPartsSchedule.ViewportElement is null) {

                newCenter = new XYZ(
                    -UnitUtilsHelper.ConvertToInternalValue(2.9) - SheetInfo.SystemPartsSchedule.ViewportHalfWidth * 2,
                    UnitUtilsHelper.ConvertToInternalValue(120) + SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2,
                    0);
            }

            (SheetInfo.SystemPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.SystemPartsSchedule.ViewportCenter = newCenter;

            return true;
        }




        internal bool PlaceIFCPartsSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.IFCPartsSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.IFCPartsSchedule.ViewportName =
                    ViewModel.IFC_PARTS_SCHEDULE_PREFIX
                        + SheetInfo.PylonKeyName
                        + ViewModel.IFC_PARTS_SCHEDULE_SUFFIX;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.IFCPartsSchedule)) {
                return false;
            }







            // Рассчитываем и задаем корректную точку вставки спецификации системных деталей пилона
            XYZ newCenter = new XYZ(
                    0,
                    UnitUtilsHelper.ConvertToInternalValue(120) + SheetInfo.IFCPartsSchedule.ViewportHalfHeight * 2,
                    0);

            if(SheetInfo.SystemPartsSchedule.ViewportElement is null) {

                newCenter = new XYZ(
                    -UnitUtilsHelper.ConvertToInternalValue(2.9) - SheetInfo.IFCPartsSchedule.ViewportHalfWidth * 2,
                    UnitUtilsHelper.ConvertToInternalValue(120) + SheetInfo.IFCPartsSchedule.ViewportHalfHeight * 2,
                    0);
            }






            (SheetInfo.IFCPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.IFCPartsSchedule.ViewportCenter = newCenter;

            return true;
        }


        internal bool PlacePylonViewport(ViewSheet viewSheet, PylonView pylonView) {

            Document doc = Repository.Document;
            // Проверяем можем ли разместить на листе видовой экран вида
            if(!Viewport.CanAddViewToSheet(doc, viewSheet.Id, pylonView.ViewElement.Id)) {
                return false;
            }


            // Скрытие категории Разразы, Оси и Уровни
            SetSectionCategoryVisibility(SheetInfo.GeneralView.ViewElement, new List<BuiltInCategory> {
                BuiltInCategory.OST_Sections,
                BuiltInCategory.OST_Grids,
                BuiltInCategory.OST_Levels
            }, false);


            pylonView.ViewElement.Scale = pylonView.ViewScale;
            pylonView.ViewElement.get_Parameter(BuiltInParameter.SECTION_COARSER_SCALE_PULLDOWN_METRIC).Set(100);

            // Размещаем сечение пилона на листе
            Viewport viewPort = null;
            try {
                viewPort = Viewport.Create(doc, viewSheet.Id, pylonView.ViewElement.Id, new XYZ(0, 0, 0));
            } catch(Exception) {
                return false;
            }

            pylonView.ViewportElement = viewPort;
            viewPort.LookupParameter("Номер вида").Set(pylonView.ViewportNumber);
            viewPort.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(pylonView.ViewportName);

            // Задание правильного типа видового экрана
            ICollection<ElementId> typesOfViewPort = viewPort.GetValidTypes();
            foreach(ElementId typeId in typesOfViewPort) {
                ElementType type = doc.GetElement(typeId) as ElementType;
                if(type == null) {
                    continue;
                }

                if(type.Name == pylonView.ViewportTypeName) {
                    viewPort.ChangeTypeId(type.Id);
                    break;
                }
            }

            // Получение габаритов видового экрана
            Outline viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y;

            pylonView.ViewportHalfWidth = viewportHalfWidth;
            pylonView.ViewportHalfHeight = viewportHalfHeight;

            // Задание правильного положения метки видового экрана
#if REVIT_2021_OR_LESS
                        
            //report += "Вы работаете в Revit 2020 или 2021, поэтому имя вида необходимо будет спозиционировать на листе самостоятельно.";
                        //report += string.Format("Вы работаете в Revit 2020 или 2021, поэтому метку имени вида \"{0}\" необходимо будет спозиционировать на листе самостоятельно" 
                        //+ Environment.NewLine, ViewElement.Name);
#else
            viewPort.LabelOffset = new XYZ(viewportHalfWidth, 2 * viewportHalfHeight - 0.022, 0);
#endif


            // Включение видимости категории Разразы, Оси и Уровни
            SetSectionCategoryVisibility(SheetInfo.GeneralView.ViewElement, new List<BuiltInCategory> {
                BuiltInCategory.OST_Sections,
                BuiltInCategory.OST_Grids,
                BuiltInCategory.OST_Levels
            }, true);

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
            GetAndWriteScheduleViewportInfo(viewSheet, pylonView, scheduleSheetInstance);

            return true;
        }


        /// <summary>
        /// Получает и запоминает точку вставки спецификации и ее габариты
        /// </summary>
        /// <param name="viewSheet"></param>
        /// <param name="pylonView"></param>
        /// <param name="viewport"></param>
        public void GetAndWriteScheduleViewportInfo(ViewSheet viewSheet, PylonView pylonView, ScheduleSheetInstance viewport) {

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




        public void SetSectionCategoryVisibility(View viewElement, List<BuiltInCategory> builtInCategories, bool doVisible) {

            ViewSection viewTemplate = Repository.Document.GetElement(viewElement.ViewTemplateId) as ViewSection;
            Categories categories = Repository.Document.Settings.Categories;

            if(viewTemplate is null) {
                // Скрытие категории Разразы, Оси и Уровни
                foreach(BuiltInCategory builtInCategory in builtInCategories) {
                    viewElement.SetCategoryHidden(categories.get_Item(builtInCategory).Id, !doVisible);
                }
            } else {
                // Скрытие категории Разразы, Оси и Уровни через шаблон вида
                foreach(BuiltInCategory builtInCategory in builtInCategories) {
                    viewTemplate.SetCategoryHidden(categories.get_Item(builtInCategory).Id, !doVisible);
                }
            }
        }
    }
}
