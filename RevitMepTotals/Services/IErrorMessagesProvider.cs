using System.Collections.Generic;

namespace RevitMepTotals.Services {
    /// <summary>
    /// Сервис, предоставляющий сообщения об ошибках при обработке документов
    /// </summary>
    internal interface IErrorMessagesProvider {
        /// <summary>
        /// Возвращает сообщение об ошибке, когда файл не может быть обработан
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileCannotBeProcessMessage(string path);

        /// <summary>
        /// Возвращает сообщение об ошибке, когда файл уже открыт и не может быть обработан
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileAlreadyOpenedMessage(string path);

        /// <summary>
        /// Возвращает сообщение об ошибке, когда в файле слишком много ошибок и он не может быть обработан
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileDataCorruptionMessage(string path);

        /// <summary>
        /// Возвращает сообщение об ошибке, когда файл удален
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileRemovedMessage(string path);

        /// <summary>
        /// Возвращает сообщение об ошибке, когда версия файла не поддерживается текущим приложением Revit
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFileVersionIsInvalidMessage(string path);

        /// <summary>
        /// Возвращает сообщение об ошибке, когда у компьютера не достаточно ресурсов, чтобы открыть файл
        /// </summary>
        /// <returns></returns>
        string GetInsufficientResourcesMessage();

        /// <summary>
        /// Возвращает сообщение об ошибке конфликта имен
        /// </summary>
        /// <param name="conflictNames">Имена, которые конфликтуют друг с другом</param>
        /// <returns></returns>
        string GetFileNamesConflictMessage(ICollection<string> conflictNames);
    }
}
