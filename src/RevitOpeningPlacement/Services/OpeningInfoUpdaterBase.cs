using System;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Базовый класс сервиса обновления информации по заданиям на отверстия и чистовым отверстиям.
/// <para>
/// Берет на себя обвязку обработки: перехват ошибок геометрии с назначением статуса ошибки
/// и сброс кэша после обработки очередного элемента.
/// </para>
/// </summary>
/// <typeparam name="T">Задание на отверстие или чистовое отверстие</typeparam>
internal abstract class OpeningInfoUpdaterBase<T> : IOpeningInfoUpdater<T> where T : class, ISolidProvider {
    public void UpdateInfo(T opening) {
        if(opening is null) {
            throw new ArgumentNullException(nameof(opening));
        }

        try {
            UpdateInfoCore(opening);
        } catch(Exception ex) when(
            ex is NullReferenceException
            or ArgumentNullException
            or InvalidOperationException
            or Autodesk.Revit.Exceptions.ApplicationException) {
            SetInvalidStatus(opening);
        } finally {
            ClearCache();
        }
    }

    /// <summary>
    /// Обновляет информацию по заданному элементу
    /// </summary>
    /// <param name="opening">Задание на отверстие или чистовое отверстие</param>
    private protected abstract void UpdateInfoCore(T opening);

    /// <summary>
    /// Назначает элементу статус ошибки обработки геометрии
    /// </summary>
    /// <param name="opening">Задание на отверстие или чистовое отверстие</param>
    private protected abstract void SetInvalidStatus(T opening);

    /// <summary>
    /// Сбрасывает кэш, накопленный при обработке очередного элемента
    /// </summary>
    private protected virtual void ClearCache() { }
}
