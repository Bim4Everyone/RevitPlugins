namespace dosymep.Revit.ServerClient {

    public enum LockState {
        /// <summary>
        /// Не блокирован.
        /// </summary>
        Unlocked,

        /// <summary>
        /// Заблокирован.
        /// </summary>
        Locked,

        /// <summary>
        /// Блокировка родителя.
        /// </summary>
        LockedParent,

        /// <summary>
        /// Блокировка дочернего объекта.
        /// </summary>
        LockedChildren,

        /// <summary>
        /// Был разблокирован.
        /// </summary>
        BeingUnlocked,

        /// <summary>
        /// Был заблокирован.
        /// </summary>
        BeingLocked
    }

    /// <summary>
    /// Базовый класс ответа Revit сервера.
    /// </summary>
    public class RevitResponse {
        /// <summary>
        /// Контекст блокировки папки или модели администратором.
        /// </summary>
        /// <remarks>Описание использования административной блокировки, такой как копирование или перемещение папок.</remarks>
        public string LockContext { get; set; }

        /// <summary>
        /// Состояние блокировки.
        /// </summary>
        public LockState LockState { get; set; }

        /// <summary>
        /// Информация о текущей блокировки модели клиентами сервера.
        /// </summary>
        public ModelLockInfo ModelLockInfo { get; set; }
    }
}