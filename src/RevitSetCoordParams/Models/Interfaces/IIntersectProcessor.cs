using System;
using System.Collections.Generic;
using System.Threading;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface IIntersectProcessor {
    /// <summary>
    /// Свойство для определения количества элементов модели в прогресс-баре.
    /// </summary>    
    /// <returns>
    /// Коллекция RevitElement - обрабатываемых элементов модели.
    /// </returns>
    IEnumerable<RevitElement> RevitElements { get; }
    /// <summary>
    /// Основной метод обработки элементов.
    /// </summary>
    /// <remarks>
    /// В данном методе происходит транзакция, пересечение элементов с объемными элементами, присвоение параметров
    /// </remarks>
    /// <param name="progress">Прогресс.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>
    /// Коллекция элементов WarningElement, необходимых для отображения в окне предупреждений
    /// </returns>
    IReadOnlyCollection<WarningElement> Run(IProgress<int> progress = null, CancellationToken ct = default);
}
