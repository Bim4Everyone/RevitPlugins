using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления окна для работы с конкретным входящим заданием на отверстие от инженера в файле архитектора или конструктора
    /// </summary>
    internal class OpeningMepTaskIncomingViewModel : BaseViewModel {
        /// <summary>
        /// Экземпляр семейства задания на отверстие
        /// </summary>
        private readonly OpeningMepTaskIncoming _openingTask;


        public OpeningMepTaskIncomingViewModel(OpeningMepTaskIncoming incomingOpeningTask) {
            if(incomingOpeningTask is null) {
                throw new ArgumentNullException(nameof(incomingOpeningTask));
            }
            _openingTask = incomingOpeningTask;

            OpeningId = _openingTask.Id.ToString();
            FileName = Path.GetFileNameWithoutExtension(incomingOpeningTask.FileName);
            Date = _openingTask.Date.Split().FirstOrDefault() ?? string.Empty;
            MepSystem = _openingTask.MepSystem;
            Description = _openingTask.Description;
            CenterOffset = _openingTask.CenterOffset;
            BottomOffset = _openingTask.BottomOffset;
            IsAccepted = _openingTask.IsAccepted;
            Diameter = _openingTask.DisplayDiameter;
            Width = _openingTask.DisplayWidth;
            Height = _openingTask.DisplayHeight;
            Thickness = _openingTask.DisplayThickness;
            FamilyShortName = _openingTask.FamilyShortName;
            HostName = _openingTask.HostName;
            Status = _openingTask.Status.GetEnumDescription();
        }


        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public string OpeningId { get; } = string.Empty;

        /// <summary>
        /// Название связанного файла-источника задания на отверстие
        /// </summary>
        public string FileName { get; } = string.Empty;

        /// <summary>
        /// Дата создания отверстия
        /// </summary>
        public string Date { get; } = string.Empty;

        /// <summary>
        /// Название инженерной системы, для элемента которой создано задание на отверстие
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
        /// Толщина
        /// </summary>
        public string Thickness { get; } = string.Empty;

        /// <summary>
        /// Статус задания на отверстие
        /// </summary>
        public string Status { get; } = string.Empty;

        /// <summary>
        /// Расположение отверстия - в перекрытии/в стене
        /// </summary>
        public string FamilyShortName { get; } = string.Empty;

        /// <summary>
        /// Название хоста задания на отверстие
        /// </summary>
        public string HostName { get; } = string.Empty;


        private bool _isAccepted;
        /// <summary>
        /// Статус задания: принято/не принято
        /// </summary>
        public bool IsAccepted {
            get => _isAccepted;
            set => RaiseAndSetIfChanged(ref _isAccepted, value);
        }

        /// <summary>
        /// Комментарий к заданию на отверстие
        /// </summary>
        private string _comment;
        public string Comment {
            get => _comment;
            set => RaiseAndSetIfChanged(ref _comment, value);
        }


        /// <summary>
        /// Возвращает экземпляр семейства задания на отверстие
        /// </summary>
        /// <returns></returns>
        public FamilyInstance GetFamilyInstance() {
            return _openingTask.GetFamilyInstance();
        }
    }
}
