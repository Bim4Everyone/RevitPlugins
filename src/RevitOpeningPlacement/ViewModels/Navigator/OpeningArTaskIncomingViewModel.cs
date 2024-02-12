using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления входящего задания на отверстие от архитектора в файле активном конструктора
    /// </summary>
    internal class OpeningArTaskIncomingViewModel : BaseViewModel, IOpeningTaskIncomingForKrViewModel, IEquatable<OpeningArTaskIncomingViewModel> {
        /// <summary>
        /// Экземпляр семейства проема АР, являющегося входящим заданием на отверстие для КР
        /// </summary>
        private readonly OpeningArTaskIncoming _openingTask;


        public OpeningArTaskIncomingViewModel(OpeningArTaskIncoming incomingOpeningTask) {
            _openingTask = incomingOpeningTask ?? throw new ArgumentNullException(nameof(incomingOpeningTask));

            OpeningId = _openingTask.Id;
            FileName = Path.GetFileNameWithoutExtension(_openingTask.FileName);
            Diameter = _openingTask.DisplayDiameter;
            Height = _openingTask.DisplayHeight;
            Width = _openingTask.DisplayWidth;
            Status = _openingTask.Status.GetEnumDescription();
            Comment = _openingTask.Comment;
        }


        public ElementId OpeningId { get; } = ElementId.InvalidElementId;

        public string FileName { get; } = string.Empty;

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
        /// Статус задания на отверстие
        /// </summary>
        public string Status { get; } = string.Empty;

        /// <summary>
        /// Комментарий экземпляра семейства задания на отверстие
        /// </summary>
        public string Comment { get; } = string.Empty;

        public override bool Equals(object obj) {
            return (obj != null)
                && (obj is OpeningArTaskIncomingViewModel vmOther)
                && Equals(vmOther);
        }

        public override int GetHashCode() {
            return (int) OpeningId.GetIdValue() + FileName.GetHashCode();
        }

        public bool Equals(OpeningArTaskIncomingViewModel other) {
            return (other != null)
                && (OpeningId == other.OpeningId)
                && FileName.Equals(other.FileName);
        }

        /// <summary>
        /// Возвращает коллекцию элементов, в которой находится входящее задание на отверстие, которое надо выделить на виде
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementModel> GetElementsToSelect() {
            return new ElementModel[] {
                new ElementModel(_openingTask.GetFamilyInstance(), _openingTask.Transform)
            };
        }

        /// <summary>
        /// Возвращает хост входящего задания на отверстие
        /// </summary>
        /// <returns></returns>
        public Element GetElementToHighlight() {
            return _openingTask.GetHost();
        }
    }
}
