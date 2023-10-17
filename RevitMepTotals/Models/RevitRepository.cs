using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;

namespace RevitMepTotals.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication ?? throw new System.ArgumentNullException(nameof(uiApplication));
        }

        public UIApplication UIApplication { get; }

        public Application Application => UIApplication.Application;
    }
}
