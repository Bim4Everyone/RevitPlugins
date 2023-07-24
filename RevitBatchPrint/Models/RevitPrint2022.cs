using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;

namespace RevitBatchPrint.Models {
    internal class RevitPrint2022 {
        private readonly RevitRepository _revitRepository;

        public RevitPrint2022(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        /// <summary>
        /// Путь до папки сохранения.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Наименование параметра альбома по которому будет фильтрация листов.
        /// </summary>
        public string FilterParamName { get; set; }

        /// <summary>
        /// Значение параметра альбома по которому будет фильтрация листов.
        /// </summary>
        public string[] FilterParamValues { get; set; }

        /// <summary>
        /// Список ошибок при выводе на печать.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

#if REVIT_2022_OR_GREATER
        public void Execute(PDFExportOptions exportParams) {
            List<ViewSheet> viewSheets = FilterParamValues
                .SelectMany(item => _revitRepository.GetViewSheets(FilterParamName, item))
                .OrderBy(sheet => sheet, new ViewSheetComparer())
                .ToList();

            // Получаем сообщение со всеми листами
            // у которых есть виды с отключенной подрезкой видов
            AddErrorCropView(viewSheets);

            try {
                _revitRepository.Document.Export(Folder, viewSheets.Select(item => item.Id).ToArray(), exportParams);
                Process.Start(Path.Combine(Folder, exportParams.FileName + ".pdf"));
            } catch(Win32Exception) {
                Errors.Add("Произошла ошибка открытия pdf файла."
                           + Environment.NewLine
                           + "Файл сохранен по следующему пути:"
                           + Environment.NewLine
                           + Path.Combine(Folder, exportParams.FileName + ".pdf"));
            } catch(Exception ex) {
                Errors.Add("Ошибка экспорта в pdf: " + ex.Message);
            }
        }
#endif

        private void AddErrorCropView(List<ViewSheet> viewSheets) {
            IEnumerable<string> messages = viewSheets
                .Select(item => (ViewSheet: item, ViewsWithoutCrop: _revitRepository.GetViewsWithoutCrop(item)))
                .Where(item => item.ViewsWithoutCrop.Count > 0)
                .Select(item => GetMessage(item.ViewSheet, item.ViewsWithoutCrop));

            if(messages.Any()) {
                Errors.Add("Листы у которые есть виды с отключенной подрезкой:" + Environment.NewLine +
                           string.Join(Environment.NewLine, messages));
            }
        }

        private string GetMessage(ViewSheet viewSheet, List<View> views) {
            string separator = Environment.NewLine + "        - ";
            return $"    {viewSheet.SheetNumber}:{separator}{string.Join(separator, views.Select(item => item.Name))}";
        }
    }
}