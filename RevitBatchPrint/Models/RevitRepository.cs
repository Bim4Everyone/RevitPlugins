using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitBatchPrint.Models {
    internal class RevitRepository {
        private readonly UIApplication _uiApplication;

        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public Document Document => _uiApplication.ActiveUIDocument.Document;
        public Application Application => _uiApplication.Application;

        /// <summary>
        /// Используемый принтер по умолчанию.
        /// </summary>
        public static string DefaultPrinterName { get; } = "PDFCreator";

        /// <summary>
        /// Наименование параметров по которым должна быть фильтрация.
        /// </summary>
        public static IReadOnlyList<string> PrintParamNames { get; set; } = new List<string>() { "Орг.ОбознчТома(Комплекта)", "Орг.КомплектЧертежей", "ADSK_Комплект чертежей" };

        /// <summary>
        /// Возвращает список возможных имен параметров группировки альбомов.
        /// </summary>
        /// <returns>Возвращает список возможных имен параметров группировки альбомов.</returns>
        public List<string> GetPrintParamNames() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterElement))
                .OfType<ParameterElement>()
                .Where(item => item.GetDefinition().ParameterType == ParameterType.Text)
                .Select(item => item.Name)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Возвращает список возможных значений параметра группировки альбомов.
        /// </summary>
        /// <param name="printParamName">Наименование параметра группировки альбомов.</param>
        /// <returns>Возвращает список возможных значений параметра группировки альбомов.</returns>
        public List<(string, int)> GetPrintParamValues(string printParamName) {
            if(string.IsNullOrEmpty(printParamName)) {
                throw new ArgumentException($"'{nameof(printParamName)}' cannot be null or empty.", nameof(printParamName));
            }

            return GetViewSheets()
                .GroupBy(item => (string) item.GetParamValueOrDefault(printParamName))
                .Where(item => !string.IsNullOrEmpty(item.Key))
                .Select(item => (item.Key, item.Count()))
                .ToList();
        }

        /// <summary>
        /// Возвращает все листы, которые разрешено печатать и не являющиеся шаблонами.
        /// </summary>
        /// <returns>Возвращает все листы, которые разрешено печатать и не являющиеся шаблонами.</returns>
        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .Where(item => item.CanBePrinted)
                .Where(item => item.IsTemplate == false)
                .Where(item => item.ViewType == ViewType.DrawingSheet)
                .ToList();
        }

        /// <summary>
        /// Возвращает отфильтрованный список листов по параметру группировки.
        /// </summary>
        /// <param name="printParamName">Наименование параметра группировки альбомов.</param>
        /// <param name="printParamValue">Значение параметра группировки альбомов</param>
        /// <returns>Возвращает отфильтрованный список листов по параметру группировки.</returns>
        public List<ViewSheet> GetViewSheets(string printParamName, string printParamValue) {
            if(string.IsNullOrEmpty(printParamName)) {
                throw new ArgumentException($"'{nameof(printParamName)}' cannot be null or empty.", nameof(printParamName));
            }

            if(string.IsNullOrEmpty(printParamValue)) {
                throw new ArgumentException($"'{nameof(printParamValue)}' cannot be null or empty.", nameof(printParamValue));
            }

            return GetViewSheets()
                .Where(item => item.GetParamValueOrDefault(printParamName, string.Empty).Equals(printParamValue))
                .ToList();
        }

        /// <summary>
        /// Возвращает основную надпись.
        /// </summary>
        /// <param name="viewSheet">Лист.</param>
        /// <returns>Возвращает основную надпись. Может быть null.</returns>
        public FamilyInstance GetTitleBlock(ViewSheet viewSheet) {
            if(viewSheet is null) {
                throw new ArgumentNullException(nameof(viewSheet));
            }

            return (FamilyInstance) new FilteredElementCollector(viewSheet.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .FirstOrDefault();
        }

        /// <summary>
        /// Возвращает настройки печати по основной надписи.
        /// </summary>
        /// <param name="familyInstance">Экземпляр основной надписи.</param>
        /// <returns>Возвращает настройки печати по основной надписи.</returns>
        public PrintSettings GetPrintSettings(FamilyInstance familyInstance) {
            if(familyInstance is null) {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            double sheetWidth = (double) familyInstance.GetParamValueOrDefault(BuiltInParameter.SHEET_WIDTH);
            double sheetHeight = (double) familyInstance.GetParamValueOrDefault(BuiltInParameter.SHEET_HEIGHT);

            sheetWidth = UnitUtils.ConvertFromInternalUnits(sheetWidth, DisplayUnitType.DUT_MILLIMETERS);
            sheetHeight = UnitUtils.ConvertFromInternalUnits(sheetHeight, DisplayUnitType.DUT_MILLIMETERS);

            return new PrintSettings() {
                Format = Format.GetFormat((int) Math.Round(sheetWidth), (int) Math.Round(sheetHeight)),
                FormatOrientation = GetFormatOrientation((int) Math.Round(sheetWidth), (int) Math.Round(sheetHeight))
            };
        }

        /// <summary>
        /// Возвращает формат листа по его имени.
        /// </summary>
        /// <param name="formatName">Наименование формата листа.</param>
        /// <returns>Возвращает размер печатаемого листа.</returns>
        public PaperSize GetPaperSizeByName(string formatName) {
            if(string.IsNullOrEmpty(formatName)) {
                throw new ArgumentException($"'{nameof(formatName)}' cannot be null or empty.", nameof(formatName));
            }

            return Document.PrintManager.PaperSizes.OfType<PaperSize>().FirstOrDefault(item => item.Name.Equals(formatName));
        }

        /// <summary>
        /// Возвращает ориентацию листа по его размеру.
        /// </summary>
        /// <param name="width">Ширина листа.</param>
        /// <param name="height">Высота листа.</param>
        /// <returns>Возвращает ориентацию листа по его размеру.</returns>
        private static PageOrientationType GetFormatOrientation(int width, int height) {
            return width > height ? PageOrientationType.Landscape : PageOrientationType.Portrait;
        }
    }
}
