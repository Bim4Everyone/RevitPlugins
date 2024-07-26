using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class SlopeCreationData {
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
