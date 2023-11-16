using System;
using System.Runtime.CompilerServices;

using Autodesk.Revit.UI;

namespace RevitClashDetective.Models.Handlers {
    internal class RevitEventHandler : IExternalEventHandler, INotifyCompletion {
        private readonly ExternalEvent _externalEvent;
        private Action _continuation;

        public RevitEventHandler() {
            _externalEvent = ExternalEvent.Create(this);
        }

        public bool IsCompleted { get; set; }
        public Action TransactAction { get; set; }
        public UIApplication App { get; set; }


        public void Execute(UIApplication app) {
            try {
                TransactAction?.Invoke();
            } finally {
                IsCompleted = true;
                _continuation?.Invoke();
            }
        }

        public string GetName() {
            return "RevitMethod";
        }

        public RevitEventHandler Raise() {
            IsCompleted = false;
            _continuation = null;
            _externalEvent.Raise();
            return this;
        }

        public void OnCompleted(Action continuation) {
            _continuation = continuation;
        }
        public RevitEventHandler GetAwaiter() {
            return this;
        }
        public void GetResult() { }
    }
}
