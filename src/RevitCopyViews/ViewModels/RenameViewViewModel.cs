using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyViews.Models;

namespace RevitCopyViews.ViewModels;

internal class RenameViewViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;

    private string _prefix;
    private string _suffix;
    private string _replaceNewText;
    private string _replaceOldText;

    private bool _isAllowReplacePrefix;
    private bool _isAllowReplaceSuffix;

    private bool _withPrefix;
    private bool _replacePrefix;
    private bool _replaceSuffix;

    private ObservableCollection<string> _prefixes;
    private ObservableCollection<string> _suffixes;

    private ObservableCollection<string> _restrictedViewNames;
    private ObservableCollection<RevitViewViewModel> _selectedViews;

    public RenameViewViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        ReplacePrefix = true;
        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string Prefix {
        get => _prefix;
        set => RaiseAndSetIfChanged(ref _prefix, value);
    }

    public string Suffix {
        get => _suffix;
        set => RaiseAndSetIfChanged(ref _suffix, value);
    }

    public bool IsAllowReplacePrefix {
        get => _isAllowReplacePrefix;
        set => RaiseAndSetIfChanged(ref _isAllowReplacePrefix, value);
    }

    public bool IsAllowReplaceSuffix {
        get => _isAllowReplaceSuffix;
        set => RaiseAndSetIfChanged(ref _isAllowReplaceSuffix, value);
    }

    public bool WithPrefix {
        get => _withPrefix;
        set => RaiseAndSetIfChanged(ref _withPrefix, value);
    }

    public bool ReplacePrefix {
        get => _replacePrefix;
        set => RaiseAndSetIfChanged(ref _replacePrefix, value);
    }

    public bool ReplaceSuffix {
        get => _replaceSuffix;
        set => RaiseAndSetIfChanged(ref _replaceSuffix, value);
    }

    public string ReplaceOldText {
        get => _replaceOldText;
        set => RaiseAndSetIfChanged(ref _replaceOldText, value);
    }

    public string ReplaceNewText {
        get => _replaceNewText;
        set => RaiseAndSetIfChanged(ref _replaceNewText, value);
    }

    public ObservableCollection<string> Prefixes {
        get => _prefixes;
        private set => RaiseAndSetIfChanged(ref _prefixes, value);
    }

    public ObservableCollection<string> Suffixes {
        get => _suffixes;
        private set => RaiseAndSetIfChanged(ref _suffixes, value);
    }

    public ObservableCollection<RevitViewViewModel> SelectedViews {
        get => _selectedViews;
        set => RaiseAndSetIfChanged(ref _selectedViews, value);
    }

    public ObservableCollection<string> RestrictedViewNames {
        get => _restrictedViewNames;
        set => RaiseAndSetIfChanged(ref _restrictedViewNames, value);
    }

    private void LoadView() {
        SelectedViews = new ObservableCollection<RevitViewViewModel>(
            _revitRepository.GetSelectedViews()
                .Select(item => new RevitViewViewModel(item)));

        RestrictedViewNames =
            new ObservableCollection<string>(_revitRepository.GetViewNames(_revitRepository.GetViews()));

        Prefixes = new ObservableCollection<string>(
            SelectedViews
                .Select(item => item.Prefix)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct());

        Suffixes = new ObservableCollection<string>(
            SelectedViews
                .Select(item => item.Suffix)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct());

        IsAllowReplacePrefix = Prefixes.Count > 0;
        IsAllowReplaceSuffix = Suffixes.Count > 0;

        ReplacePrefix = IsAllowReplacePrefix && ReplacePrefix;
        ReplaceSuffix = IsAllowReplaceSuffix && ReplaceSuffix;

        if(Prefixes.Count == 1) {
            Prefix = Prefixes.First();
        }

        if(Suffixes.Count == 1) {
            Suffix = Suffixes.First();
        }
    }

    private void AcceptView() {
        using var transaction =
            _revitRepository.StartTransaction(_localizationService.GetLocalizedString("RenameView.TransactionName"));

        foreach(var revitView in SelectedViews) {
            revitView.View.Name = GetViewName(revitView);
        }

        transaction.Commit();
    }

    private bool CanAcceptView() {
        string[] generatingNames = SelectedViews
            ?.Select(GetViewName)
            .ToArray() ?? [];

        string generateName = generatingNames.GroupBy(item => item)
            .Where(item => item.Count() > 1)
            .Select(item => item.Key)
            .FirstOrDefault();

        if(!string.IsNullOrEmpty(generateName)) {
            ErrorText = _localizationService.GetLocalizedString("RenameViewWindow.FoundRepeatViewName", generateName);
            return false;
        }

        string existingName =
            generatingNames.FirstOrDefault(item => RestrictedViewNames?.Any(item.Equals) == true);

        if(!string.IsNullOrEmpty(existingName)) {
            ErrorText = _localizationService.GetLocalizedString("RenameViewWindow.FoundExistsViewName", existingName);
            return false;
        }

        ErrorText = null;
        return true;
    }

    private string GetViewName(RevitViewViewModel revitView) {
        string originalName = revitView.OriginalName;
        if(!string.IsNullOrEmpty(ReplaceOldText)) {
            originalName = revitView.OriginalName.Replace(ReplaceOldText, ReplaceNewText);
        }

        if(!WithPrefix) {
            return originalName;
        }

        var splitViewOptions = new SplitViewOptions {
            ReplacePrefix = ReplacePrefix,
            ReplaceSuffix = ReplaceSuffix
        };

        var splittedViewName = revitView.SplitName(originalName, splitViewOptions);
        splittedViewName.Prefix = Prefix;
        splittedViewName.Suffix = Suffix;

        return Delimiter.CreateViewName(splittedViewName);
    }
}
