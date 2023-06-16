using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

using static System.Net.Mime.MediaTypeNames;

namespace RevitPylonDocumentation.Models {
    public class PylonView {
        internal PylonView(MainViewModel mvm) {
            mainViewModel = mvm;
        }

        internal MainViewModel mainViewModel { get; set; }

        public bool InProject { get; set; } = false;
        public bool InProjectEditableInGUI { get; set; } = true;
        public bool OnSheet { get; set; } = false;
        public bool OnSheetEditableInGUI { get; set; } = true;


        public View ViewElement { get; set; }

        public Element ViewportElement { get; set; }

        public string ViewportNumber { get; set; }

        public string ViewportName { get; set; }

        public string ViewportTypeName { get; set; }

        public double ViewportHalfWidth { get; set; }

        public double ViewportHalfHeight { get; set; }

        public XYZ ViewportCenter { get; set; }




        public string PlacePylonViewport(Document doc, PylonSheetInfo pylonSheetInfo) {
            string report = string.Empty;
            // Запись полученных значений ведем в pylonView, а потом его выведем в return 
            // и присвоем в нужный вид в pylonSheetInfo
            if(ViewElement == null) {
                #region Отчет
                report += string.Format("   Произошла ошибка! Метод по размещению вида не получил элемент вида у {0}" + Environment.NewLine, pylonSheetInfo.PylonKeyName);
                #endregion
                return report;
            }

            // Проверяем можем ли разместить на листе видовой экран вида
            if(!Viewport.CanAddViewToSheet(doc, pylonSheetInfo.PylonViewSheet.Id, ViewElement.Id)) {
                #region Отчет
                report += string.Format("   Произошла ошибка! Нельзя разместить вид \"{0}\" на листе \"{1}\"" + Environment.NewLine, ViewElement.Name, pylonSheetInfo.PylonViewSheet.Name);
                #endregion
                return report;
            }

            // Размещаем сечение пилона на листе
            Viewport viewPort = null;
            try {
                // Размещаем спецификацию арматуры пилона на листе
                viewPort = Viewport.Create(doc, pylonSheetInfo.PylonViewSheet.Id, ViewElement.Id, new XYZ(0, 0, 0));
            } catch(Exception) {
                #region Отчет
                report += string.Format("   Произошла ошибка! Не удалось разместить вид у {0}" + Environment.NewLine, pylonSheetInfo.PylonKeyName);
                #endregion
                return report;
            }

            viewPort.LookupParameter("Номер вида").Set(ViewportNumber);
            ViewportElement = viewPort;

            // Задание правильного типа видового экрана
            ICollection<ElementId> typesOfViewPort = viewPort.GetValidTypes();
            foreach(ElementId typeId in typesOfViewPort) {
                ElementType type = doc.GetElement(typeId) as ElementType;
                if(type == null) {
                    continue;
                }

                if(type.Name == ViewportTypeName) {
                    viewPort.ChangeTypeId(type.Id);
                    break;
                }
            }


            // Получение габаритов видового экрана
            Outline viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y;

            // Задание правильного положения метки видового экрана
#if REVIT_2021_OR_LESS 
            report += "Вы работаете в Revit 2020 или 2021, поэтому имя вида необходимо будет спозиционировать на листе самостоятельно.";
            report += string.Format("Вы работаете в Revit 2020 или 2021, поэтому метку имени вида \"{0}\" необходимо будет спозиционировать на листе самостоятельно" 
            + Environment.NewLine, ViewElement.Name);
#else
            viewPort.LabelOffset = new XYZ(viewportHalfWidth, 2 * viewportHalfHeight - 0.022, 0);
#endif

            ViewportHalfWidth = viewportHalfWidth;
            ViewportHalfHeight = viewportHalfHeight;


            return report;
        }



        public string PlaceScheduleViewport(Document doc, PylonSheetInfo pylonSheetInfo) {
            string report = string.Empty;

            // Запись полученных значений ведем в pylonView, а потом его выведем в return 
            // и присвоем в нужный вид в pylonSheetInfo

            if(ViewElement == null) {
                #region Отчет
                report += string.Format("   Произошла ошибка! Метод по размещению спецификации не получил элемент спецификации у {0}" + Environment.NewLine, pylonSheetInfo.PylonKeyName);
                #endregion
                return report;
            }

            ScheduleSheetInstance scheduleSheetInstance = null;
            try {
                // Размещаем спецификацию арматуры пилона на листе
                scheduleSheetInstance = ScheduleSheetInstance.Create(doc, pylonSheetInfo.PylonViewSheet.Id, ViewElement.Id, new XYZ(0, 0, 0));
            } catch(Exception) {
                #region Отчет
                report += string.Format("   Произошла ошибка! Не удалось разместить спецификацию у {0}" + Environment.NewLine, pylonSheetInfo.PylonKeyName);
                #endregion
                return report;
            }

            ViewportElement = scheduleSheetInstance;


            // Получение габаритов видового экрана спецификации
            BoundingBoxXYZ boundingBoxXYZ = scheduleSheetInstance.get_BoundingBox(pylonSheetInfo.PylonViewSheet);
            double scheduleHalfWidth = boundingBoxXYZ.Max.X / 2;
            double scheduleHalfHeight = -boundingBoxXYZ.Min.Y / 2;     // Создается так, что верхний левый угол спеки в нижнем правом углу рамки

            ViewportHalfWidth = scheduleHalfWidth;
            ViewportHalfHeight = scheduleHalfHeight;

            return report;
        }
    }
}
