using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models {
    internal class FilterElemsByExportService {
        private readonly Document _doc;

        public FilterElemsByExportService(Document doc) {
            _doc = doc;
        }

        /// <summary>
        /// Получает элементы, видимые на виде при помощи функции экспорта
        /// </summary>
        public List<Element> GetElements() {
            View activeView = _doc.ActiveView;
            List<Element> allExportedElements = new List<Element>();

            var exportContext = new ExportContext(_doc);
            CustomExporter exporter = new CustomExporter(_doc, exportContext) {
                IncludeGeometricObjects = true,
                ShouldStopOnError = false
            };
            exporter.Export(activeView);

            allExportedElements.AddRange(exportContext.ExportedElements);
            return allExportedElements;
        }
    }
}
