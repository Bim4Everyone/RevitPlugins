using System;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IMessenger {
    void Send<TMessage>(TMessage message);

    void Subscribe<TMessage>(object subscriber, Action<TMessage> handler);

    void Unsubscribe<TMessage>(object subscriber);
}
