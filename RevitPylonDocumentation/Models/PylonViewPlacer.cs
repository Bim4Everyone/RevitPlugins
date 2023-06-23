using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

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

            // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
            if(SheetInfo.GeneralView.ViewportHalfHeight * 2 > SheetInfo.TitleBlockHeight) {
                SheetInfo.SetTitleBlockSize(ViewModel._revitRepository.Document, 2, 1);
            }

            // Рассчитываем и задаем корректную точку вставки основного вида пилона
            XYZ newCenter = new XYZ(
                -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth + 0.065,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralView.ViewportHalfHeight - 0.016,
                0);
            (SheetInfo.GeneralView.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.GeneralView.ViewportCenter = newCenter;

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

            // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона
            XYZ newCenter;

            if(SheetInfo.GeneralView.OnSheet) {
                newCenter = new XYZ(
                -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth * 2
                    + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + 0.065,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralView.ViewportHalfHeight - 0.016,
                0);
            }

            XYZ newCenter = new XYZ(
                -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth * 2 
                    + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + 0.065,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralView.ViewportHalfHeight - 0.016,
                0);
            (SheetInfo.GeneralView.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.GeneralView.ViewportCenter = newCenter;

            return true;
        }












        //internal bool PlaceTransverseFirstViewPorts() {
        //    double coordinateX = 0;
        //    double coordinateY = 0;

        //    // Проверям вдруг вид не создался
        //    if(SheetInfo.TransverseViewFirst.ViewElement == null) {
        //        return false;
        //    } else {
        //        // Заполнеяем данные для задания
        //        SheetInfo.TransverseViewFirst.ViewportTypeName = "Сечение_Номер вида";
        //        SheetInfo.TransverseViewFirst.ViewportNumber = "1";
        //        SheetInfo.TransverseViewFirst.ViewportName = "";
        //    }


        //    double titleBlockOffset = UnitUtilsHelper.ConvertToInternalValue(20);


        //    // Размещение поперечного сечения 1-1 (потому что он ниже)

        //    if(SheetInfo.TransverseViewFirst.ViewElement != null) {
        //        // Передаем первое поперечное сечение пилона
        //        string answer = TransverseViewFirst.PlacePylonViewport(ViewModel._revitRepository.Document, this);

        //        if(!PlacePylonViewport(ViewModel._revitRepository.Document, SheetInfo)) {
        //            return false;
        //        }






        //        if(answer.Length > 0) {
        //            ViewModel.Report = answer;
        //        }

        //        if(TransverseViewFirst.ViewportElement is null) {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tРабота с размещением поперечного вида пилона 1-1 на листе \"{0}\" прервана", PylonViewSheet.Name);
        //            #endregion
        //            return;
        //        } else {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид 1-1 пилона \"{0}\" успешно размещен на листе \"{1}\"", TransverseViewFirst.ViewElement.Name, PylonViewSheet.Name);
        //            #endregion
        //        }

        //        // Рассчитываем и задаем корректную точку вставки первого попереченого вида пилона
        //        XYZ newCenterFirst = new XYZ(
        //            -TitleBlockWidth + titleBlockOffset + GeneralView.ViewportHalfWidth * 2 + TransverseViewFirst.ViewportHalfWidth,
        //            0.015 + TransverseViewFirst.ViewportHalfHeight,
        //            0);
        //        (TransverseViewFirst.ViewportElement as Viewport).SetBoxCenter(newCenterFirst);

        //        TransverseViewFirst.ViewportCenter = newCenterFirst;
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПоперечный вид 1-1 пилона \"{0}\" спозиционирован", PylonViewSheet.Name);
        //        #endregion
        //    } else {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПроизошла ошибка! Не найден поперечный вид пилона \"{0}\" для листа \"{1}\"",
        //            ViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX, PylonViewSheet.Name);
        //        #endregion
        //    }
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 1-1 - работа завершена";
        //    #endregion




        //    return;
        //}

















        internal bool PlacePylonViewport(ViewSheet viewSheet, PylonView pylonView) {

            Document doc = Repository.Document;
            // Проверяем можем ли разместить на листе видовой экран вида
            if(!Viewport.CanAddViewToSheet(doc, viewSheet.Id, pylonView.ViewElement.Id)) {
                return false;
            }

            // Размещаем сечение пилона на листе
            Viewport viewPort = null;
            try {
                viewPort = Viewport.Create(doc, viewSheet.Id, pylonView.ViewElement.Id, new XYZ(0, 0, 0));
            } catch(Exception) {
                return false;
            }

            viewPort.LookupParameter("Номер вида").Set(pylonView.ViewportNumber);
            viewPort.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(pylonView.ViewportName);
            pylonView.ViewportElement = viewPort;

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

            //            // Задание правильного положения метки видового экрана
            //#if REVIT_2021_OR_LESS
            //            report += "Вы работаете в Revit 2020 или 2021, поэтому имя вида необходимо будет спозиционировать на листе самостоятельно.";
            //            report += string.Format("Вы работаете в Revit 2020 или 2021, поэтому метку имени вида \"{0}\" необходимо будет спозиционировать на листе самостоятельно" 
            //            + Environment.NewLine, ViewElement.Name);
            //#else
            //            viewPort.LabelOffset = new XYZ(viewportHalfWidth, 2 * viewportHalfHeight - 0.022, 0);
            //#endif



            return true;
        }


    }
}
