using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления окна для работы с конкретным исходящим заданием на отверстие в файле инженера
    /// </summary>
    internal class OpeningMepTaskOutcomingViewModel : BaseViewModel {
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
        /// Возвращает экземпляр семейства задания на отверстие
        /// </summary>
        /// <returns></returns>
        public FamilyInstance GetFamilyInstance() {
            return _openingTask.GetFamilyInstance();
        }
    }
}
