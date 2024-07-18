using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models.Report {
    public class ReportItem {
        public ReportItem(string paramName, ElementId elementId) {
            ErrorName = $"Отсутствует параметр \"{paramName}\"";
            ElementIdsAsString = elementId.ToString();
            ElementIds.Add(elementId);
        }

        public bool IsCheck { get; set; } = false;

        public string ErrorName { get; set; }

        public string ElementIdsAsString { get; set; }

        public List<ElementId> ElementIds { get; set; } = new List<ElementId>();


        /// <summary>
        /// Добавляет id элемента в список ошибочных, если его там еще нет
        /// </summary>
        public void AddId(ElementId elementId) {
            if(!ElementIds.Contains(elementId)) {
                ElementIds.Add(elementId);
                ElementIdsAsString += $", {elementId}";
            }
        }
    }
}
