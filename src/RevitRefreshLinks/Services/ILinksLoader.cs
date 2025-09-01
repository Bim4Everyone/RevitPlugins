using System;
using System.Collections.Generic;
using System.Threading;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface ILinksLoader {
    /// <summary>
    /// Добавляет заданные связи в проект и возвращает информацию об ошибках, если они есть.
    /// </summary>
    /// <param name="links">Связи для добавления в проект.</param>
    /// <param name="progress">Прогресс добавления.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Ошибки обновления, если они есть.</returns>
    ICollection<(ILink Link, string Error)> AddLinks(
        ICollection<ILink> links,
        IProgress<int> progress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Обновляет заданные связи в проекте и возвращает информацию об ошибках, если они есть.
    /// </summary>
    /// <param name="links">Связи для обновления.</param>
    /// <param name="progress">Прогресс добавления.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Ошибки обновления, если они есть.</returns>
    ICollection<(ILink Link, string Error)> UpdateLinks(
        ICollection<ILinkPair> links,
        IProgress<int> progress = null,
        CancellationToken ct = default);
}
