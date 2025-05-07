using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitFinishingWalls.Models {
    /// <summary>
    /// Класс, в котором содержатся данные для построения стены
    /// </summary>
    internal class WallCreationData {

        public WallCreationData(Document document, Room room) {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Room = room ?? throw new ArgumentNullException(nameof(room));
            ElementsForJoin = [];
        }


        public Document Document { get; }

        /// <summary>
        /// Осевая линия стены
        /// </summary>
        public Curve Curve { get; set; }

        public ElementId WallTypeId { get; set; }

        /// <summary>
        /// Отметка верха стены от уровня низа в единицах Revit
        /// </summary>
        public double TopElevation { get; set; }

        /// <summary>
        /// Смещение снизу от <see cref="LevelId"/> в единицах Revit
        /// </summary>
        public double BaseOffset { get; set; }

        /// <summary>
        /// Коллекция элементов, с которыми нужно соединить созданную стену
        /// </summary>
        public ICollection<Element> ElementsForJoin { get; }

        /// <summary>
        /// Помещение, в котором будет создана отделочная стена
        /// </summary>
        public Room Room { get; }


        /// <summary>
        /// Добавляет коллекцию элементов в <see cref="ElementsForJoin"/>
        /// </summary>
        /// <param name="elements"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRangeElementsForJoin(ICollection<Element> elements) {
            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }

            foreach(Element element in elements) {
                if(element != null && !ElementsForJoin.Any(el => el.Id == element.Id)) {
                    ElementsForJoin.Add(element);
                }
            }
        }
    }
}
