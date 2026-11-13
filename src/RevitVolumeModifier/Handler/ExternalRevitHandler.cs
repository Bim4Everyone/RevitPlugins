using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

namespace RevitVolumeModifier.Handler;

internal class ExternalRevitHandler : IExternalEventHandler {
    private readonly object _sync = new();
    private readonly Queue<Action<UIApplication>> _actions = new();
    private readonly ExternalEvent _externalEvent;

    public ExternalRevitHandler() {
        _externalEvent = ExternalEvent.Create(this);
    }

    public void Raise(Action<UIApplication> action) {
        if(action == null) {
            throw new ArgumentNullException(nameof(action));
        }

        lock(_sync) {
            _actions.Enqueue(action);
        }
        _externalEvent.Raise();
    }

    public Task RaiseAsync(Action<UIApplication> action) {
        if(action == null) {
            throw new ArgumentNullException(nameof(action));
        }

        var tcs = new TaskCompletionSource<object>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Raise(app => {
            try {
                action(app);
                tcs.SetResult(null);
            } catch(Exception ex) {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public Task<T> RaiseAsync<T>(Func<UIApplication, T> func) {
        if(func == null) {
            throw new ArgumentNullException(nameof(func));
        }

        var tcs = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Raise(app => {
            try {
                var result = func(app);
                tcs.SetResult(result);
            } catch(Exception ex) {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public void Execute(UIApplication app) {
        while(true) {
            Action<UIApplication> action;

            lock(_sync) {
                if(_actions.Count == 0) {
                    return;
                }
                action = _actions.Dequeue();
            }

            action?.Invoke(app);
        }
    }

    public string GetName() {
        return nameof(ExternalRevitHandler);
    }
}
