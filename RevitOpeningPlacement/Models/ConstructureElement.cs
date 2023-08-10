using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models {
    /// <summary>
    /// Класс для обертки элементов конструкций из активного документа ревита, например стен и перекрытий
    /// </summary>
    internal class ConstructureElement : ISolidProvider {
        private readonly Element _element;

        /// <summary>
        /// Закэшированный солид
        /// </summary>
        private Solid _solid;

        /// <summary>
        /// Закэшированный ББ
        /// </summary>
        private BoundingBoxXYZ _boundingBox;


        /// <summary>
        /// Конструктор класса для обертки элементов конструкций из активного документа ревита, например стен и перекрытий
        /// </summary>
        /// <param name="constructureElement"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConstructureElement(Element constructureElement) {
            if(constructureElement is null) {
                throw new ArgumentNullException(nameof(constructureElement));
            }
            _element = constructureElement;
            Id = _element.Id.IntegerValue;
            FileName = _element.Document.PathName;

            SetTransformedBBoxXYZ();
            SetSolid();
        }


        /// <summary>
        /// Id элемента
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Название файла, в котором находится элемент
        /// </summary>
        public string FileName { get; } = string.Empty;


        /// <summary>
        /// Возвращает солид элемента
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Solid GetSolid() {
            return _solid;
        }

        /// <summary>
        /// Возвращает ББ элемента
        /// </summary>
        /// <returns></returns>
        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _boundingBox;
        }


        private void SetSolid() {
            _solid = _element.GetSolid();
        }

        private void SetTransformedBBoxXYZ() {
            _boundingBox = _element.GetBoundingBox();
        }
    }
}
