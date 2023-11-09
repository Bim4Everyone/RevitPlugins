using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления чистового отверстия в навигаторе АР по входящим заданиям на отверстия.
    /// Использовать для отображения чистовых отверстий АР из активного документа, которые требуют внимания архитектора
    /// </summary>
    internal class OpeningRealArViewModel : BaseViewModel, ISelectorAndHighlighter, IEquatable<OpeningRealArViewModel> {
        private readonly OpeningRealAr _openingReal;


        public OpeningRealArViewModel(OpeningRealAr openingReal) {
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
        public ElementId OpeningId { get; } = ElementId.InvalidElementId;

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
                && (obj is OpeningRealArViewModel otherVM)
                && Equals(otherVM);
        }

        public bool Equals(OpeningRealArViewModel other) {
            return (other != null)
                && (OpeningId == other.OpeningId);
        }

        public override int GetHashCode() {
            return (int) OpeningId.GetIdValue();
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
        public ICollection<ElementModel> GetElementsToSelect() {
            return new ElementModel[] {
                new ElementModel(_openingReal.GetFamilyInstance())
            };
        }
    }
}
