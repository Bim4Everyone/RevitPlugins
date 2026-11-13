using System;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления исходящего задания на отверстие от ВИС в файле ВИС.
/// </summary>
internal interface IOpeningMepTaskOutcomingViewModel : ISelectorAndHighlighter {
    /// <summary>
    /// Id экземпляра семейства задания на отверстие
    /// </summary>
    string OpeningId { get; }

    /// <summary>
    /// Дата создания отверстия
    /// </summary>
    string Date { get; }

    /// <summary>
    /// Название инженерной системы, для элемента которой создан экземпляр семейства задания на отверстие
    /// </summary>
    string MepSystem { get; }

    /// <summary>
    /// Описание задания на отверстие
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Отметка центра задания на отверстие
    /// </summary>
    string CenterOffset { get; }

    /// <summary>
    /// Отметка низа задания на отверстие
    /// </summary>
    string BottomOffset { get; }

    /// <summary>
    /// Статус задания на отверстие
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Комментарий
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// Имя пользователя, создавшего задание на отверстие
    /// </summary>
    string Username { get; }
}
