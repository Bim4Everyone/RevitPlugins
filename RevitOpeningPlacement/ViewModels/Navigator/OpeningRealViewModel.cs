using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления чистового отверстия в навигаторе АР по входящим заданиям на отверстия.
    /// Использовать для отображения чистовых отверстий, которые требуют внимания архитектора
    /// </summary>
    internal class OpeningRealViewModel : BaseViewModel, ISelectorAndHighlighter, IEquatable<OpeningRealViewModel> {
        private readonly OpeningReal _openingReal;


        public OpeningRealViewModel(OpeningReal openingReal) {
            _openingReal = openingReal ?? throw new System.ArgumentNullException(nameof(openingReal));

            OpeningId = _openingReal.Id;
            Diameter = _openingReal.Diameter;
            Width = _openingReal.Width;
            Height = _openingReal.Height;
            Status = _openingReal.Status.GetEnumDescription();
            Comment = _openingReal.Comment;
        }


        /// <summary>
        /// Id экземпляра семейства чистового на отверстия
        /// </summary>
        public int OpeningId { get; } = -1;

        /// <summary>
        /// Диаметр
        /// </summary>
        public string Diameter { get; } = string.Empty;

        /// <summary>
        /// Ширина
        /// </summary>
        public string Width { get; } = string.Empty;

        /// <summary>
        /// Высота
        /// </summary>
        public string Height { get; } = string.Empty;

        /// <summary>
        /// Статус чистового отверстия
        /// </summary>
        public string Status { get; } = string.Empty;

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment { get; } = string.Empty;


        public override bool Equals(object obj) {
            return (obj != null)
                && (obj is OpeningRealViewModel otherVM)
                && Equals(otherVM);
        }

        public bool Equals(OpeningRealViewModel other) {
            return (other != null)
                && (OpeningId == other.OpeningId);
        }

        public override int GetHashCode() {
            return OpeningId;
        }

        /// <summary>
        /// Возвращает хост чистового отверстия
        /// </summary>
        /// <returns></returns>
        public Element GetElementToHighlight() {
            return _openingReal.GetHost();
        }

        /// <summary>
        /// Возвращает коллекцию, в которой находится чистовое отверстие, которое надо выделить на виде
        /// </summary>
        /// <returns></returns>
        public ICollection<Element> GetElementsToSelect() {
            return new Element[] {
                _openingReal.GetFamilyInstance()
            };
        }
    }
}
