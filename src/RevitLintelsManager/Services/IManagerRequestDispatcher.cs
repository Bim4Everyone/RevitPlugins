using System;
using System.Threading.Tasks;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Контракт очереди чтения и безопасного освобождения ExternalEvent.</summary>
/// <remarks>Kernel вызывает Dispose после TryDisposeInApiContext; при ошибке до старта — в API-контексте команды.</remarks>
internal interface IManagerRequestDispatcher : IDisposable {
    /// <summary>Разрешает запросы после успешного открытия окна сессии.</summary>
    void StartAcceptingRequests();

    /// <summary>Ставит чтение контекста в очередь ExternalEvent без обращения к модели из GUI.</summary>
    /// <param name="request">Token исходного документа и поколение запроса.</param>
    /// <returns>Независимые значения; отменённая задача при устаревании/закрытии/блокировке.</returns>
    /// <exception cref="ArgumentNullException">Запрос не задан.</exception>
    /// <exception cref="InvalidOperationException">Revit отказал в сигнале или чтение завершилось ошибкой.</exception>
    Task<ManagerDocumentContext> Enqueue(ManagerRequest request);

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
