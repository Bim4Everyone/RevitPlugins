using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Extensions;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления чистового отверстия в навигаторе КР по входящим заданиям на отверстия.
    /// Использовать для отображения чистовых отверстий КР из активного документа, которые требуют внимания конструктора
    /// </summary>
    internal class OpeningRealKrViewModel : BaseViewModel, ISelectorAndHighlighter, IEquatable<OpeningRealKrViewModel> {
        private readonly OpeningRealKr _openingReal;


        public OpeningRealKrViewModel(OpeningRealKr openingReal) {
            _openingReal = openingReal ?? throw new ArgumentNullException(nameof(openingReal));

            OpeningId = _openingReal.Id;
            Diameter = _openingReal.Diameter;
            Width = _openingReal.Width;
            Height = _openingReal.Height;
            Status = _openingReal.Status.GetDescription();
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
                && (obj is OpeningRealKrViewModel otherVM)
                && Equals(otherVM);
        }

        public override int GetHashCode() {
            return (int) OpeningId.GetIdValue();
        }

        public bool Equals(OpeningRealKrViewModel other) {
            return (other != null)
                && (OpeningId == other.OpeningId);
        }

        public ICollection<ElementModel> GetElementsToSelect() {
            return new ElementModel[] {
                new ElementModel( _openingReal.GetFamilyInstance())
            };
        }

        public Element GetElementToHighlight() {
            return _openingReal.GetHost();
        }
    }
}
