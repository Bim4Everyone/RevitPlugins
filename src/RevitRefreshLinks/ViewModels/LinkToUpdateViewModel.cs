using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels;
internal class LinkToUpdateViewModel : BaseViewModel {
    private readonly RevitLinkType _linkType;
    private readonly ObservableCollection<ILink> _sourceLinks;
    private bool _isSelected;
    private bool _canSelect;
    private string _sourceStatus;
    private string _displayWorksetStatus;
    private string _displayLinkStatus;

    public LinkToUpdateViewModel(RevitLinkType linkType) {
        _linkType = linkType ?? throw new System.ArgumentNullException(nameof(linkType));
        _sourceLinks = [];
        NameWithExtension = _linkType.Name;
        LinkStatus = _linkType.GetLinkedFileStatus();
        WorksetIsClosed = LinkStatus == LinkedFileStatus.InClosedWorkset;

        _sourceLinks.CollectionChanged += SourceLinksCollectionChanged;
    }


    public string NameWithExtension { get; }

    public bool WorksetIsClosed { get; }

    public LinkedFileStatus LinkStatus { get; }

    public int SourceLinksCount => _sourceLinks.Count;

    public bool IsSelected {
        get => _isSelected;
        set {
            if(CanSelect) {
                RaiseAndSetIfChanged(ref _isSelected, value);
            }
        }
    }

    public string SourceStatus {
        get => _sourceStatus;
        set => RaiseAndSetIfChanged(ref _sourceStatus, value);
    }

    public string DisplayWorksetStatus {
        get => _displayWorksetStatus;
        set => RaiseAndSetIfChanged(ref _displayWorksetStatus, value);
    }

    public string DisplayLinkStatus {
        get => _displayLinkStatus;
        set => RaiseAndSetIfChanged(ref _displayLinkStatus, value);
    }

    public bool CanSelect {
        get => _canSelect;
        private set => RaiseAndSetIfChanged(ref _canSelect, value);
    }


    public void AddSourceLinks(ICollection<ILink> links) {
        if(links is null) {
            throw new ArgumentNullException(nameof(links));
        }
        foreach(var link in links) {
            _sourceLinks.Add(link);
        }
    }

    public void ClearSourceLinks() {
        _sourceLinks.Clear();
    }

    public IReadOnlyCollection<ILink> GetSourceLinks() {
        return new ReadOnlyCollection<ILink>(_sourceLinks);
    }

    public RevitLinkType GetLinkType() {
        return _linkType;
    }

    private void SourceLinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        CanSelect = !WorksetIsClosed && (SourceLinksCount == 1);
        if(!CanSelect) {
            IsSelected = false;
        }
    }
}
