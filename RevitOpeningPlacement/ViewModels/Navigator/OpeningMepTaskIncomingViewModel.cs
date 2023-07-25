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

            OpeningId = _openingTask.Id.ToString();
            FileName = Path.GetFileNameWithoutExtension(incomingOpeningTask.FileName);
            Date = _openingTask.Date;
            MepSystem = _openingTask.MepSystem;
            Description = _openingTask.Description;
            CenterOffset = _openingTask.CenterOffset;
            BottomOffset = _openingTask.BottomOffset;
            IsAccepted = _openingTask.IsAccepted;
        }


        public string OpeningId { get; } = string.Empty;

        public string FileName { get; } = string.Empty;

        public string Date { get; } = string.Empty;

        public string MepSystem { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public string CenterOffset { get; } = string.Empty;

        public string BottomOffset { get; } = string.Empty;

        private bool _isAccepted;
        public bool IsAccepted {
            get => _isAccepted;
            set => RaiseAndSetIfChanged(ref _isAccepted, value);
        }

        private string _comment;
        public string Comment {
            get => _comment;
            set => RaiseAndSetIfChanged(ref _comment, value);
        }


        public FamilyInstance GetFamilyInstance() {
            return _openingTask.GetFamilyInstance();
        }
    }
}
