using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyExplorer.Models {
    internal class RevitExternalEvent : IExternalEventHandler, INotifyCompletion {
        private readonly string _externalEventName;
        private readonly ExternalEvent _externalEvent;

        private Action _continuation;

        public RevitExternalEvent(string externalEventName) {
            _externalEventName = externalEventName;
            _externalEvent = ExternalEvent.Create(this);
        }

        public string TransactionName { get; set; }
        public Action<UIApplication> ExternalAction { get; set; }

        public string GetName() {
            return _externalEventName;
        }

        public void Execute(UIApplication app) {
            try {
                if(ExternalAction == null) {
                    return;
                }

                using(var transaction = new Transaction(app.ActiveUIDocument.Document)) {
                    transaction.Start(TransactionName);

                    ExternalAction(app);

                    transaction.Commit();
                }
            } finally {
                IsCompleted = true;
                _continuation?.Invoke();
            }
        }

        public RevitExternalEvent Raise() {
            IsCompleted = false;
            _continuation = null;

            _externalEvent.Raise();
            return this;
        }

        public bool IsCompleted { get; private set; }

        public void OnCompleted(Action continuation) {
            _continuation = continuation;
        }

        public void GetResult() { }
        public RevitExternalEvent GetAwaiter() {
            return this;
        }
    }
}
