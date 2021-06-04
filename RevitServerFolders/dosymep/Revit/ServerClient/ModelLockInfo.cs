using System;

namespace dosymep.Revit.ServerClient {
    /// <summary>
    /// Типы блокировок модели.
    /// </summary>
    public enum ModelLockTypes {
        /// <summary>
        /// Блокировка файла.
        /// </summary>
        Data,

        /// <summary>
        /// Блокировка разрешений.
        /// </summary>
        Permissions
    }

    /// <summary>
    /// Опции блокировки модели.
    /// </summary>
    public enum ModelLockOptions {
        /// <summary>
        /// Только чтение.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Чтение-запись.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Чтение или запись.
        /// </summary>
        NonExclusiveReadOrWrite = 128
    }

    /// <summary>
    /// Информация о блокировки модели.
    /// </summary>
    public class ModelLockInfo {
        /// <summary>
        /// Время блокировки.
        /// </summary>
        public TimeSpan Age { get; set; }

        /// <summary>
        /// Время начала блокировки.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Имя пользователя заблокировавшего модель.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Путь до модели.
        /// </summary>
        public string ModelPath { get; set; }

        /// <summary>
        /// Тип блокировки.
        /// </summary>
        public ModelLockTypes ModelLockType { get; set; }

        /// <summary>
        /// Опции блокировки.
        /// </summary>
        public ModelLockOptions ModelLockOptions { get; set; }
    }
}