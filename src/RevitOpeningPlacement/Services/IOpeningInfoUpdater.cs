using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис для обновления информации по заданиям на отверстия и чистовым отверстиям
/// </summary>
internal interface IOpeningInfoUpdater<T> where T : class, ISolidProvider {
    /// <summary>
    /// Обновляет информацию по заданному элементу
    /// </summary>
    /// <param name="opening">Задание на отверстие или чистовое отверстие</param>
    void UpdateInfo(T opening);
}
