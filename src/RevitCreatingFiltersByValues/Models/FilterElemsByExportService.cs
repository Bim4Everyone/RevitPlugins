using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCreatingFiltersByValues.Models {
    internal class FilterElemsByExportService {
        private readonly Document _doc;

        public FilterElemsByExportService(Document doc) {
            _doc = doc;
        }

        /// <summary>
        /// Получает элементы, видимые на виде при помощи функции экспорта
        /// </summary>
        /// <returns></returns>
        public List<Element> GetElemetns() {
            View activeView = _doc.ActiveView;
            // Список для хранения всех экспортированных элементов
            List<Element> allExportedElements = new List<Element>();

            var linkInstances = new FilteredElementCollector(_doc, activeView.Id)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();

            using(Transaction transaction = _doc.StartTransaction("Временное скрытие/изоляция")) {
                // Для корректного получения элементов на виде (в т.ч. из связанных файлов),
                // чтобы избежать дублирования необходимо сначала временно скрыть все экземпляры
                // связанных файлов и произвести экспорт того, что есть в текущем файле.
                // Затем вернуть отображение связей, изолировать их по одной и производить экспорт
                activeView.HideElements(linkInstances.Select(x => x.Id).ToList());

                var exportContext = new ExportContext(_doc);
                CustomExporter exporter = new CustomExporter(_doc, exportContext) {
                    IncludeGeometricObjects = true,
                    ShouldStopOnError = false
                };
                exporter.Export(activeView);

                allExportedElements.AddRange(exportContext.ExportedElements);
                activeView.UnhideElements(linkInstances.Select(x => x.Id).ToList());

                foreach(var linkInstance in linkInstances) {
                    activeView.IsolateElementsTemporary(new List<ElementId> { linkInstance.Id });

                    exportContext = new ExportContext(linkInstance.GetLinkDocument());
                    exporter = new CustomExporter(_doc, exportContext) {
                        IncludeGeometricObjects = true,
                        ShouldStopOnError = false
                    };
                    exporter.Export(activeView);

                    allExportedElements.AddRange(exportContext.ExportedElements);
                    activeView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                }
                // Манипуляции со скрытием/отображением необходимо производить под транзакцией
                // но в дальнейшем она не нужна, поэтому сбрасываем ее
                transaction.RollBack();
            }
            return allExportedElements;
        }
    }
}
