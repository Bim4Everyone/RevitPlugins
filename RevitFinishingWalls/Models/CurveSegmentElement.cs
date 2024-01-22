using System;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.Models {
    /// <summary>
    /// Класс для хранения линии сегмента границы помещения и элементе, который этот сегмент образует
    /// </summary>
    internal class CurveSegmentElement {
        public CurveSegmentElement(Element element, Curve curve) {
            Curve = curve ?? throw new ArgumentNullException(nameof(curve));
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }


        /// <summary>
        /// Линия сегмента границы помещения, по которой нужно будет создать отделочную стену
        /// при условии, что привязкой стены будет чистовая наружная поверхность
        /// </summary>
        public Curve Curve { get; }

        /// <summary>
        /// Элемент, который образует линию сегмента границы помещения, с которым нужно будет соединить отделочную стену
        /// </summary>
        public Element Element { get; }
    }
}
