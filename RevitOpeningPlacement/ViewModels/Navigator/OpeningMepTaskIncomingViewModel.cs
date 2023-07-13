using System;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningMepTaskIncomingViewModel : BaseViewModel {
        private readonly OpeningMepTaskIncoming _openingTask;


        public OpeningMepTaskIncomingViewModel(OpeningMepTaskIncoming incomingOpeningTask) {
            if(incomingOpeningTask is null) {
                throw new ArgumentNullException(nameof(incomingOpeningTask));
            }
            _openingTask = incomingOpeningTask;

            FileName = Path.GetFileNameWithoutExtension(incomingOpeningTask.FileName);
            Date = _openingTask.Date;
            MepSystem = _openingTask.MepSystem;
            Description = _openingTask.Description;
            CenterOffset = _openingTask.CenterOffset;
            BottomOffset = _openingTask.BottomOffset;
        }


        public string FileName { get; } = string.Empty;

        public string Date { get; } = string.Empty;

        public string MepSystem { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public string CenterOffset { get; } = string.Empty;

        public string BottomOffset { get; } = string.Empty;


        public FamilyInstance GetFamilyInstance() {
            return _openingTask.GetFamilyInstance();
        }
    }
}
