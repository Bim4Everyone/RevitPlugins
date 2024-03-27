using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopeCreationData {
        public SlopeCreationData(Document document) {
            Document = document ?? throw new System.ArgumentNullException(nameof(document));
        }
        public Document Document { get; }

        /// <summary>
        /// Высота откоса
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Центр проема
        /// </summary>
        public XYZ Center { get; set; }

        /// <summary>
        /// Ширина откоса
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Глубина откоса
        /// </summary>
        public double Depth { get; set; }

        /// <summary>
        /// Типоразмер размещаемого откоса
        /// </summary>
        public ElementId SlopeTypeId { get; set; }

        /// <summary>
        /// Угол поворота откоса
        /// </summary>
        public double RotationRadiansAngle { get; set; }
    }
}
