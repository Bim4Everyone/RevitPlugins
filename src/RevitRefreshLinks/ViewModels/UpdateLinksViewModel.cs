using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels {
    internal class UpdateLinksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ITwoSourceLinksProvider _linksProvider;
        private readonly ILinksLoader _linksLoader;
        private string _errorText;
        private string _selectedLocalSource;
        private string _selectedServerSource;


        public UpdateLinksViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService,
            ITwoSourceLinksProvider linksProvider,
            ILinksLoader linksLoader) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _linksProvider = linksProvider ?? throw new ArgumentNullException(nameof(linksProvider));
            _linksLoader = linksLoader ?? throw new ArgumentNullException(nameof(linksLoader));
            LinksToUpdate = new ObservableCollection<LinkToUpdateViewModel>();
            SourceLocalLinks = new ObservableCollection<ILink>();
            SourceServerLinks = new ObservableCollection<ILink>();

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            SelectLocalSourceCommand = RelayCommand.Create(SelectLocalSource);
            SelectServerSourceCommand = RelayCommand.Create(SelectServerSource);
            SelectAllLinksCommand = RelayCommand.Create(SelectAllLinks, CanSelectAny);
            UnselectAllLinksCommand = RelayCommand.Create(UnselectAllLinks, CanSelectAny);
            InvertSelectedLinksCommand = RelayCommand.Create(InvertSelectedLinks, CanSelectAny);
        }


        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ICommand SelectLocalSourceCommand { get; }

        public ICommand SelectServerSourceCommand { get; }

        public ICommand SelectAllLinksCommand { get; }

        public ICommand UnselectAllLinksCommand { get; }

        public ICommand InvertSelectedLinksCommand { get; }

        public ObservableCollection<LinkToUpdateViewModel> LinksToUpdate { get; }

        public ObservableCollection<ILink> SourceLocalLinks { get; }

        public ObservableCollection<ILink> SourceServerLinks { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SelectedLocalSource {
            get => _selectedLocalSource;
            set => RaiseAndSetIfChanged(ref _selectedLocalSource, value);
        }

        public string SelectedServerSource {
            get => _selectedServerSource;
            set => RaiseAndSetIfChanged(ref _selectedServerSource, value);
        }


        private void LoadView() {
            var links = _revitRepository
                .GetExistingLinks()
                .Select(t => new LinkToUpdateViewModel(t))
                .OrderBy(t => t.NameWithExtension);
            foreach(var link in links) {
                link.DisplayLinkStatus = GetLinkStatus(link.LinkStatus);
                link.DisplayWorksetStatus = GetWorksetStatus(link.WorksetIsClosed);
                link.SourceStatus = GetSourceStatus(link.SourceLinksCount);
                LinksToUpdate.Add(link);
            }
        }

        private void AcceptView() {
            // TODO add logic to begin update
        }

        private bool CanAcceptView() {
            if(string.IsNullOrWhiteSpace(SelectedLocalSource) && string.IsNullOrWhiteSpace(SelectedServerSource)) {
                ErrorText = _localizationService.GetLocalizedString("UpdateLinksWindow.EmptySourcesCheck");
                return false;
            }
            if(LinksToUpdate.All(l => !l.IsSelected)) {
                ErrorText = _localizationService.GetLocalizedString("UpdateLinksWindow.EmptySelectionCheck");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void InvertSelectedLinks() {
            foreach(var link in LinksToUpdate) {
                link.IsSelected = !link.IsSelected;
            }
        }


        private bool CanSelectAny() {
            return LinksToUpdate.Any(l => l.CanSelect);
        }

        private void UnselectAllLinks() {
            foreach(var link in LinksToUpdate) {
                link.IsSelected = false;
            }
        }

        private void SelectAllLinks() {
            foreach(var link in LinksToUpdate) {
                link.IsSelected = true;
            }
        }

        private void SelectServerSource() {
            try {
                var result = _linksProvider.GetServerLinks();
                SelectedServerSource = result.SourceName;
                ResetServerLinks(result.Links);
            } catch(OperationCanceledException) {
                // pass
            }
        }

        private void SelectLocalSource() {
            try {
                var result = _linksProvider.GetLocalLinks();
                SelectedLocalSource = result.SourceName;
                ResetLocalLinks(result.Links);
            } catch(OperationCanceledException) {
                // pass
            }
        }

        private void UpdateLinkSources() {
            var localSources = SourceLocalLinks
                .GroupBy(l => l.NameWithExtension)
                .ToDictionary(g => g.Key, g => g.ToArray());
            var serverSources = SourceServerLinks
                .GroupBy(l => l.NameWithExtension)
                .ToDictionary(g => g.Key, g => g.ToArray());

            foreach(var item in LinksToUpdate) {
                item.ClearSourceLinks();
                if(localSources.TryGetValue(item.NameWithExtension, out var linksLocal)) {
                    item.AddSourceLinks(linksLocal);
                }
                if(serverSources.TryGetValue(item.NameWithExtension, out var linksServer)) {
                    item.AddSourceLinks(linksServer);
                }
                item.SourceStatus = GetSourceStatus(item.SourceLinksCount);
            }
        }

        private void ResetLocalLinks(ICollection<ILink> links) {
            SourceLocalLinks.Clear();
            foreach(ILink link in links) {
                SourceLocalLinks.Add(link);
            }
            UpdateLinkSources();
        }

        private void ResetServerLinks(ICollection<ILink> links) {
            SourceServerLinks.Clear();
            foreach(ILink link in links) {
                SourceServerLinks.Add(link);
            }
            UpdateLinkSources();
        }

        private string GetSourceStatus(int sourceCount) {
            if(sourceCount == 0) {
                return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Source.NotFound");
            } else if(sourceCount == 1) {
                return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Source.Found");
            } else if(sourceCount > 1) {
                return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Source.Duplicated");
            } else {
                throw new ArgumentException(nameof(sourceCount));
            }
        }

        private string GetLinkStatus(LinkedFileStatus status) {
            switch(status) {
                case LinkedFileStatus.Loaded:
                    return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.Loaded");
                case LinkedFileStatus.Unloaded:
                    return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.Unloaded");
                case LinkedFileStatus.NotFound:
                    return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.NotFound");
                case LinkedFileStatus.LocallyUnloaded:
                    return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.LocallyUnloaded");
                case LinkedFileStatus.InClosedWorkset:
                    return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.ClosedWorkset");
                default:
                    return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.Unknown");
            }
        }

        private string GetWorksetStatus(bool isClosed) {
            return isClosed
                ? _localizationService.GetLocalizedString("UpdateLinksWindow.List.Workset.Closed")
                 : _localizationService.GetLocalizedString("UpdateLinksWindow.List.Workset.Opened");
        }
    }
}
