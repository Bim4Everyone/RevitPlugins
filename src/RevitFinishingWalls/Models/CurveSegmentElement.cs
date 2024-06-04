using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.Models {
    /// <summary>
    /// Класс для хранения линии сегмента границы помещения и элементе, который этот сегмент образует
    /// </summary>
    internal class CurveSegmentElement {
        public CurveSegmentElement(ICollection<Element> elements, Curve curve) {
            Curve = curve ?? throw new ArgumentNullException(nameof(curve));
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }


        /// <summary>
        /// Линия сегмента границы помещения, по которой нужно будет создать отделочную стену
        /// при условии, что привязкой стены будет чистовая наружная поверхность
        /// </summary>
        public Curve Curve { get; }

        /// <summary>
        /// Элемент(ы), который(ые) образует линию сегмента границы помещения, 
        /// с которыми нужно будет соединить отделочную стену
        /// </summary>
        public ICollection<Element> Elements { get; }
    }
}
