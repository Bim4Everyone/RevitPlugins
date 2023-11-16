using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.RevitClashReport {
    internal abstract class BaseClashesLoader {
        protected BaseClashesLoader() { }


        /// <summary>
        /// Возвращает Id или InvalidElementId, если не удалось преобразовать строку
        /// </summary>
        private protected ElementId GetId(string idString) {
            if(string.IsNullOrWhiteSpace(idString)) { return ElementId.InvalidElementId; }

            var match = Regex.Match(idString, @"(?'id'\d+)");
            if(match.Success) {
                string stringId = match.Groups["id"].Value;
#if REVIT_2023_OR_LESS
                bool successParse = int.TryParse(stringId, out int numericId);
#else
                bool successParse = long.TryParse(stringId, out long numericId);
#endif
                if(successParse) {
                    return new ElementId(numericId);
                } else {
                    return ElementId.InvalidElementId;
                }
            }
            return ElementId.InvalidElementId;
        }
    }
}