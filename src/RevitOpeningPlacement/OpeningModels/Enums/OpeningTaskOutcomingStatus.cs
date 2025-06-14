using System.ComponentModel;

namespace RevitOpeningPlacement.OpeningModels.Enums {
    /// <summary>
    /// Статусы исходящих заданий на отверстия
    /// </summary>
    internal enum OpeningTaskOutcomingStatus {
        /// <summary>
        /// Имеет основание - элемент, для которого было создано задание, пересекает его 
        /// и это пересечение не вызывает сомнений в корректности расположения задания на отверстие
        /// </summary>
        [Description("Корректно")]
        Correct,
        /// <summary>
        /// Слишком большой процент объема пересечения задания на отверстие и элемента ВИС относительно объема задания за отверстие
        /// </summary>
        [Description("Слишком маленькое задание")]
        TooSmall,
        /// <summary>
        /// Неточное основание - элемент, для которого было создано это задание, пересекает его, но это пересечение относительно мало
        /// </summary>
        [Description("Слишком большое задание")]
        TooBig,
        /// <summary>
        /// Задание на отверстие пересекается с другим заданием
        /// </summary>
        [Description("Пересекающееся задание")]
        Intersects,
        /// <summary>
        /// Не имеет основания в виде пересекающего его элемента, для которого и было создано задание;
        /// либо задание на отверстие на пересекает ни один элемент конструкции, например, из-за сдвинутой стены
        /// </summary>
        [Description("Неактуальное задание")]
        NotActual,
        /// <summary>
        /// Произошла ошибка обработки геометрии в процессе определения статуса
        /// </summary>
        [Description("Ошибка обработки геометрии")]
        Invalid,
        /// <summary>
        /// Задание на отверстие размещено вручную
        /// </summary>
        [Description("Размещено вручную")]
        ManuallyPlaced,
        /// <summary>
        /// Задание на отверстие - объединенное
        /// </summary>
        [Description("Объединенное")]
        United,
        /// <summary>
        /// Задание на отверстие находится в разных конструкциях
        /// </summary>
        [Description("В разных конструкциях")]
        DifferentConstructions,
        /// <summary>
        /// Задание на отверстие находится в недопустимых конструкциях
        /// </summary>
        [Description("Недопустимые конструкции")]
        UnacceptableConstructions
    }
}
