using System;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Передаёт сообщения между компонентами плагина.</summary>
internal interface IMessenger {
    /// <summary>Отправляет сообщение подписчикам.</summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="message">Отправляемое сообщение.</param>
    void Send<TMessage>(TMessage message);

    /// <summary>Подписывает объект на сообщения указанного типа.</summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="subscriber">Объект подписчика.</param>
    /// <param name="handler">Обработчик сообщения.</param>
    void Subscribe<TMessage>(object subscriber, Action<TMessage> handler);

    /// <summary>Удаляет подписку объекта на сообщения указанного типа.</summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="subscriber">Объект подписчика.</param>
    void Unsubscribe<TMessage>(object subscriber);
}
