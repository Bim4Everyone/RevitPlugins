using System;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IMessenger {
    /// <summary>Отправляет сообщение подписчикам.</summary>
    void Send<TMessage>(TMessage message);

    /// <summary>Подписывает объект на сообщения указанного типа.</summary>
    void Subscribe<TMessage>(object subscriber, Action<TMessage> handler);

    /// <summary>Удаляет подписку объекта на сообщения указанного типа.</summary>
    void Unsubscribe<TMessage>(object subscriber);
}
