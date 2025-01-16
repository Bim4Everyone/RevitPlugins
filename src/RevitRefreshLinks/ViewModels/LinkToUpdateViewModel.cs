using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRefreshLinks.ViewModels {
    internal class LinkToUpdateViewModel : BaseViewModel {
        private readonly RevitLinkType _linkType;
        private bool _isSelected;
        private string _sourceStatus;
        private string _displayWorkset;
        private string _displayLinkStatus;

        public LinkToUpdateViewModel(RevitLinkType linkType) {
            _linkType = linkType ?? throw new System.ArgumentNullException(nameof(linkType));
            Name = _linkType.Name;
            LinkStatus = _linkType.GetLinkedFileStatus();
            WorksetIsClosed = LinkStatus == LinkedFileStatus.InClosedWorkset;
        }


        public string Name { get; }

        public bool WorksetIsClosed { get; }

        public LinkedFileStatus LinkStatus { get; }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string SourceStatus {
            get => _sourceStatus;
            set => RaiseAndSetIfChanged(ref _sourceStatus, value);
        }

        public string DisplayWorkset {
            get => _displayWorkset;
            set => RaiseAndSetIfChanged(ref _displayWorkset, value);
        }

        public string DisplayLinkStatus {
            get => _displayLinkStatus;
            set => RaiseAndSetIfChanged(ref _displayLinkStatus, value);
        }
    }
}
