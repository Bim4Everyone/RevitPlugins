using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyExplorer.Models {
    internal class RevitExternalEvent : IExternalEventHandler {
        private readonly string _externalEventName;

        public RevitExternalEvent(string externalEventName) {
            _externalEventName = externalEventName;
        }

        public string TransactionName { get; set; }
        public Action<UIApplication> ExternalAction { get; set; }

        public string GetName() {
            return _externalEventName;
        }

        public void Execute(UIApplication app) {
            if(ExternalAction == null) {
                return;
            }

            using(var transaction = new Transaction(app.ActiveUIDocument.Document)) {
                transaction.Start(TransactionName);

                ExternalAction(app);

                transaction.Commit();
            }
        }
    }
}
