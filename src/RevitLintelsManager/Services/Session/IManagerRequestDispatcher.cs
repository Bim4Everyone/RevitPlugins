using System;
using System.Threading;
using System.Threading.Tasks;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Контракт очереди чтения и навигации и безопасного освобождения ExternalEvent.</summary>
/// <remarks>Kernel вызывает Dispose после TryDisposeInApiContext; при ошибке до старта — в API-контексте команды.</remarks>
internal interface IManagerRequestDispatcher : IDisposable {
    /// <summary>Признак собственного изменения вида во время API-вызова.</summary>
    bool IsPreparingManagerView { get; }

    /// <summary>Разрешает первый запрос и последующие обновления сессии.</summary>
    void StartAcceptingRequests();

    /// <summary>Ставит чтение или навигацию в очередь ExternalEvent из GUI.</summary>
    /// <param name="request">Идентификатор документа, поколение и данные операции.</param>
    /// <returns>Снимок чтения или null после перехода. Отмена при устаревании, блокировке или закрытии.</returns>
    /// <exception cref="ArgumentNullException">Запрос не задан.</exception>
    /// <exception cref="InvalidOperationException">Сигнал, чтение или навигация завершились ошибкой.</exception>
    Task<ManagerDocumentContext> Enqueue(ManagerRequest request);

    /// <summary>Передаёт утверждённый пакет одному Execute. Задача завершается после возврата callback.</summary>
    Task<ProcessingBatchResult> EnqueueBatch(ApprovedProcessingPlan approved, ManagerScope scope,
        CancellationToken token, IProgress<int> progress);

    /// <summary>Отменяет ожидающие результаты, сохраняя возможность следующих запросов.</summary>
    void InvalidateRequests();

    /// <summary>Запрещает новые запросы и отменяет все ожидающие результаты.</summary>
    void BeginClose();

    /// <summary>Сверяет native pending и повторяет потерянный сигнал на Idling в API-контексте.</summary>
    void PumpInApiContext();

    /// <summary>Освобождает ExternalEvent в API-контексте после завершения pending/active callback.</summary>
    /// <returns>True, если native-ресурс уже освобождён; иначе нужно дождаться следующего callback.</returns>
    bool TryDisposeInApiContext();
}
