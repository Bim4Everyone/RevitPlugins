using System;
using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Services;

internal class Messenger : IMessenger {
    private readonly Dictionary<Type, List<Subscription>> _subscriptions = [];

    public void Send<TMessage>(TMessage message) {
        var messageType = typeof(TMessage);

        if(!_subscriptions.TryGetValue(messageType, out var subscriptions)) {
            return;
        }

        foreach(var subscription in subscriptions.ToArray()) {
            if(message != null) {
                subscription.Handler(message);
            }
        }
    }

    public void Subscribe<TMessage>(object subscriber, Action<TMessage> handler) {

        var messageType = typeof(TMessage);

        if(!_subscriptions.TryGetValue(messageType, out var subscriptions)) {
            subscriptions = [];
            _subscriptions.Add(messageType, subscriptions);
        }

        subscriptions.Add(
            new Subscription(
                subscriber,
                message => handler((TMessage)message)));
    }

    public void Unsubscribe<TMessage>(object subscriber) {
        var messageType = typeof(TMessage);

        if(!_subscriptions.TryGetValue(messageType, out var subscriptions)) {
            return;
        }

        subscriptions.RemoveAll(
            subscription => ReferenceEquals(
                subscription.Subscriber,
                subscriber));

        if(subscriptions.Count == 0) {
            _subscriptions.Remove(messageType);
        }
    }

    private sealed class Subscription {
        public Subscription(
            object subscriber,
            Action<object> handler) {

            Subscriber = subscriber;
            Handler = handler;
        }

        public object Subscriber { get; }

        public Action<object> Handler { get; }
    }
}
