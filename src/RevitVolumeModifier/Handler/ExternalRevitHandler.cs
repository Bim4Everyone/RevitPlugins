using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

namespace RevitVolumeModifier.Handler;
internal class ExternalRevitHandler : IExternalEventHandler {
    private readonly Queue<Action<UIApplication>> _actions = new();
    private readonly ExternalEvent _externalEvent;

    public ExternalRevitHandler() {
        _externalEvent = ExternalEvent.Create(this);
    }

    public void Raise(Action<UIApplication> action) {
        _actions.Enqueue(action);
        _externalEvent.Raise();
    }

    public void Execute(UIApplication app) {
        while(_actions.Count > 0) {
            var action = _actions.Dequeue();
            action?.Invoke(app);
        }
    }

    public string GetName() {
        return nameof(ExternalRevitHandler);
    }
}
