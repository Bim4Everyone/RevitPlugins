using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления окна для работы с конкретным исходящим заданием на отверстие в активном файле инженера
    /// </summary>
    internal class OpeningMepTaskOutcomingViewModel : BaseViewModel, ISelectorAndHighlighter, IEquatable<OpeningMepTaskOutcomingViewModel> {
        /// <summary>
        /// Входящее задание на отверстие
        /// </summary>
        private readonly OpeningMepTaskOutcoming _openingTask;


        public OpeningMepTaskOutcomingViewModel(OpeningMepTaskOutcoming incomingOpeningTask) {
            if(incomingOpeningTask is null) {
                throw new ArgumentNullException(nameof(incomingOpeningTask));
            }
            _openingTask = incomingOpeningTask;

            OpeningId = _openingTask.Id.ToString();
            Date = _openingTask.Date.Split().FirstOrDefault() ?? string.Empty;
            MepSystem = _openingTask.MepSystem;
            Description = _openingTask.Description;
            CenterOffset = _openingTask.CenterOffset;
            BottomOffset = _openingTask.BottomOffset;
            Comment = _openingTask.Comment;
            Username = _openingTask.Username;

            Status = _openingTask.Status.GetEnumDescription();
        }


        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public string OpeningId { get; } = string.Empty;

        /// <summary>
        /// Дата создания отверстия
        /// </summary>
        public string Date { get; } = string.Empty;

        /// <summary>
        /// Название инженерной системы, для элемента которой создан экземпляр семейства задания на отверстие
        /// </summary>
        public string MepSystem { get; } = string.Empty;

        /// <summary>
        /// Описание задания на отверстие
        /// </summary>
        public string Description { get; } = string.Empty;

        /// <summary>
        /// Отметка центра задания на отверстие
        /// </summary>
        public string CenterOffset { get; } = string.Empty;

        /// <summary>
        /// Отметка низа задания на отверстие
        /// </summary>
        public string BottomOffset { get; } = string.Empty;

        /// <summary>
        /// Статус задания на отверстие
        /// </summary>
        public string Status { get; } = string.Empty;

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment { get; } = string.Empty;

        /// <summary>
        /// Имя пользователя, создавшего задание на отверстие
        /// </summary>
        public string Username { get; } = string.Empty;

        public override bool Equals(object obj) {
            return (obj != null)
                && (obj is OpeningMepTaskOutcomingViewModel otherVM)
                && Equals(otherVM);
        }

        public override int GetHashCode() {
            return (int) _openingTask.Id.GetIdValue();
        }

        public bool Equals(OpeningMepTaskOutcomingViewModel other) {
            return (other != null)
                && (_openingTask.Id == other._openingTask.Id);
        }

        /// <summary>
        /// Возвращает хост исходящего задания на отверстие
        /// </summary>
        /// <returns></returns>
        public Element GetElementToHighlight() {
            return _openingTask.Host;
        }

        /// <summary>
        /// Возвращает коллекцию элементов, в которой находится исходящее задание на отверстие, которое надо выделить на виде
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementModel> GetElementsToSelect() {
            return new ElementModel[] {
                new ElementModel(_openingTask.GetFamilyInstance())
            };
        }
    }
}
