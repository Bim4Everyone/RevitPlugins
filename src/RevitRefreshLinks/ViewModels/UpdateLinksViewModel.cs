using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels;
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
        ILinksLoader linksLoader,
        IProgressDialogFactory progressFactory) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _linksProvider = linksProvider ?? throw new ArgumentNullException(nameof(linksProvider));
        _linksLoader = linksLoader ?? throw new ArgumentNullException(nameof(linksLoader));
        ProgressDialogFactory = progressFactory ?? throw new ArgumentNullException(nameof(progressFactory));
        LinksToUpdate = [];
        SourceLocalLinks = [];
        SourceServerLinks = [];

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        SelectLocalSourceCommand = RelayCommand.Create(SelectLocalSource, CanSelectLocalSource);
        SelectServerSourceCommand = RelayCommand.CreateAsync(SelectServerSource);
        SelectAllLinksCommand = RelayCommand.Create(SelectAllLinks, CanSelectAny);
        UnselectAllLinksCommand = RelayCommand.Create(UnselectAllLinks, CanSelectAny);
        InvertSelectedLinksCommand = RelayCommand.Create(InvertSelectedLinks, CanSelectAny);
    }


    public IProgressDialogFactory ProgressDialogFactory { get; }

    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public ICommand SelectLocalSourceCommand { get; }

    public IAsyncCommand SelectServerSourceCommand { get; }

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
        var linkPairs = LinksToUpdate
            .Where(t => t.IsSelected && t.SourceLinksCount == 1)
            .Select(t => new LinkPair(t.GetLinkType(), t.GetSourceLinks().First()))
            .ToArray();
        ICollection<(ILink Link, string Error)> errors;
        using(var pb = ProgressDialogFactory.CreateDialog()) {
            pb.MaxValue = linkPairs.Length;
            var progress = pb.CreateProgress();
            var ct = pb.CreateCancellationToken();
            pb.Show();

            errors = _linksLoader.UpdateLinks(linkPairs, progress, ct);
        }
        if(errors?.Count > 0) {
            string msg = string.Join("\n\n",
                errors.GroupBy(e => e.Error)
                .Select(e => $"{e.Key}:\n{string.Join("\n", e.Select(i => i.Link.Name))}"));
            GetPlatformService<IMessageBoxService>()
                .Show(msg,
                _localizationService.GetLocalizedString("MessageBox.Title.ErrorUpdateLink"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }

    private bool CanAcceptView() {
        if(SelectServerSourceCommand.IsExecuting) {
            ErrorText = _localizationService.GetLocalizedString("DirectoriesExplorer.ExecutingCmdCheck");
            return false;
        }
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
        return !SelectServerSourceCommand.IsExecuting && LinksToUpdate.Any(l => l.CanSelect);
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

    private async Task SelectServerSource() {
        try {
            var result = await _linksProvider.GetServerLinksAsync();
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

    private bool CanSelectLocalSource() {
        return !SelectServerSourceCommand.IsExecuting;
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
        foreach(var link in links) {
            SourceLocalLinks.Add(link);
        }
        UpdateLinkSources();
    }

    private void ResetServerLinks(ICollection<ILink> links) {
        SourceServerLinks.Clear();
        foreach(var link in links) {
            SourceServerLinks.Add(link);
        }
        UpdateLinkSources();
    }

    private string GetSourceStatus(int sourceCount) {
        if(sourceCount == 0) {
            return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Source.NotFound");
        } else if(sourceCount == 1) {
            return _localizationService.GetLocalizedString("UpdateLinksWindow.List.Source.Found");
        } else {
            return sourceCount > 1
                ? _localizationService.GetLocalizedString("UpdateLinksWindow.List.Source.Duplicated")
                : throw new ArgumentException(nameof(sourceCount));
        }
    }

    private string GetLinkStatus(LinkedFileStatus status) {
        return status switch {
            LinkedFileStatus.Loaded => _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.Loaded"),
            LinkedFileStatus.Unloaded => _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.Unloaded"),
            LinkedFileStatus.NotFound => _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.NotFound"),
            LinkedFileStatus.LocallyUnloaded => _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.LocallyUnloaded"),
            LinkedFileStatus.InClosedWorkset => _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.ClosedWorkset"),
            _ => _localizationService.GetLocalizedString("UpdateLinksWindow.List.Link.Unknown"),
        };
    }

    private string GetWorksetStatus(bool isClosed) {
        return isClosed
            ? _localizationService.GetLocalizedString("UpdateLinksWindow.List.Workset.Closed")
             : _localizationService.GetLocalizedString("UpdateLinksWindow.List.Workset.Opened");
    }
}
