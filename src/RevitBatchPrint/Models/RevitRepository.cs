using System;
using System.Collections.Generic;
using System.IO;
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
            PrintManager = Document.PrintManager;
        }

        public Document Document => _uiApplication.ActiveUIDocument.Document;
        public Application Application => _uiApplication.Application;

        public PrintManager PrintManager { get; private set; }

        /// <summary>
        /// Используемый принтер по умолчанию.
        /// </summary>
        public static string DefaultPrinterName { get; } = "PDFCreator";

        /// <summary>
        /// Наименование параметров по которым должна быть фильтрация.
        /// </summary>
        public static IReadOnlyList<string> PrintParamNames { get; set; } = new List<string>() {
            "Орг.ОбознчТома(Комплекта)", "Орг.КомплектЧертежей", "ADSK_Комплект чертежей"
        };

        /// <summary>
        /// Перезагружает менеджер принтера.
        /// </summary>
        /// <param name="printerName">Наименование принтера.</param>
        public void ReloadPrintSettings(string printerName) {
            PrintManager = Document.PrintManager;

            // После выбора принтера все настройки сбрасываются
            PrintManager.SelectNewPrintDriver(printerName);
            PrintManager.PrintToFile = true;
            PrintManager.PrintOrderReverse = false;

            // Должно быть установлено это значение
            // если установлено другое значение
            // имя файла устанавливается ревитом
            PrintManager.PrintRange = PrintRange.Current;

            PrintManager.Apply();
            UpdatePrintFileName(null);
        }

        /// <summary>
        /// Обновляет название выходного файла для виртуального принтера.
        /// </summary>
        /// <param name="viewSheet">Печатаемый лист.</param>
        public void UpdatePrintFileName(ViewSheet viewSheet) {
            string documentFileName = string.IsNullOrEmpty(Document.Title) ? "Без имени" : Document.Title;

            string viewName = ReplaceInvalidChars(viewSheet?.SheetNumber);
            string fileName = string.IsNullOrEmpty(viewName)
                ? documentFileName
                : $"{documentFileName} ({viewName} - {ReplaceInvalidChars(viewSheet.Name)})";
            PrintManager.PrintToFileName =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName + ".pdf");
            if(File.Exists(PrintManager.PrintToFileName)) {
                File.Delete(PrintManager.PrintToFileName);
            }
        }

        public List<View> GetViewsWithoutCrop(ViewSheet viewSheet) {
            return viewSheet.GetAllPlacedViews()
                .Select(item => Document.GetElement(item))
                .OfType<View>()
                .Where(item => !item.CropBoxActive)
                .ToList();
        }

        /// <summary>
        /// Возвращает список возможных имен параметров группировки альбомов.
        /// </summary>
        /// <returns>Возвращает список возможных имен параметров группировки альбомов.</returns>
        public List<string> GetPrintParamNames() {
            var categoryId = new ElementId(BuiltInCategory.OST_Sheets);
            return Document.GetParameterBindings()
                .Where(item => item.Binding is InstanceBinding)
                .Where(item =>
                    ((InstanceBinding) item.Binding).Categories.OfType<Category>()
                    .Any(category => category.Id == categoryId))
#if REVIT_2021_OR_LESS
                .Where(item => item.Definition.ParameterType == ParameterType.Text)
#else
                .Where(item => item.Definition.GetDataType() == SpecTypeId.String.Text)
#endif
                .Select(item => item.Definition.Name)
                .OrderBy(item => item)
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
                throw new ArgumentException($"'{nameof(printParamName)}' cannot be null or empty.",
                    nameof(printParamName));
            }

            return GetViewSheets()
                .GroupBy(item => (string) item.GetParamValueOrDefault(printParamName))
                .Where(item => !string.IsNullOrEmpty(item.Key))
                .Select(item => (item.Key, item.Count()))
                .OrderBy(item => item.Key)
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
                throw new ArgumentException($"'{nameof(printParamName)}' cannot be null or empty.",
                    nameof(printParamName));
            }

            if(string.IsNullOrEmpty(printParamValue)) {
                throw new ArgumentException($"'{nameof(printParamValue)}' cannot be null or empty.",
                    nameof(printParamValue));
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

#if REVIT_2020_OR_LESS
            sheetWidth = UnitUtils.ConvertFromInternalUnits(sheetWidth, DisplayUnitType.DUT_MILLIMETERS);
            sheetHeight = UnitUtils.ConvertFromInternalUnits(sheetHeight, DisplayUnitType.DUT_MILLIMETERS);
#else
            sheetWidth = UnitUtils.ConvertFromInternalUnits(sheetWidth, UnitTypeId.Millimeters);
            sheetHeight = UnitUtils.ConvertFromInternalUnits(sheetHeight, UnitTypeId.Millimeters);
#endif

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

            return PrintManager.PaperSizes.OfType<PaperSize>().FirstOrDefault(item => item.Name.Equals(formatName));
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

        /// <summary>
        /// Заменяет запрещенные символы в имени файла на "_".
        /// </summary>
        /// <param name="filename">Наименование файла.</param>
        /// <returns>Возвращает новое наименование файла без запрещенных символов.</returns>
        private static string ReplaceInvalidChars(string filename) {
            if(string.IsNullOrEmpty(filename)) {
                return null;
            }

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}