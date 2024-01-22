using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.Models {
    /// <summary>
    /// Класс, в котором содержатся данные для построения стены
    /// </summary>
    internal class WallCreationData {

        public WallCreationData(Document document) {
            Document = document ?? throw new System.ArgumentNullException(nameof(document));
            ElementsForJoin = new List<Element>();
        }


        public Document Document { get; }

        /// <summary>
        /// Линия чистовой наружной грани стены
        /// </summary>
        public Curve Curve { get; set; }

        public ElementId LevelId { get; set; }

        public ElementId WallTypeId { get; set; }

        /// <summary>
        /// Высота в единицах Revit
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Смещение снизу в единицах Revit
        /// </summary>
        public double BaseOffset { get; set; }

        /// <summary>
        /// Коллекция элементов, с которыми нужно соединить созданную стену
        /// </summary>
        public ICollection<Element> ElementsForJoin { get; }
    }
}
