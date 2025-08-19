using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyViews.Models;

namespace RevitCopyViews.ViewModels;

internal class CopyViewViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;

    private bool _copyWithDetail;
    private bool _isAllowReplacePrefix;
    private bool _isAllowReplaceSuffix;

    private string _prefix;
    private string _suffix;
    private string _groupView;

    private bool _replacePrefix;
    private bool _replaceSuffix;
    private bool _withElevation;

    private ObservableCollection<string> _prefixes;
    private ObservableCollection<string> _suffixes;
    private ObservableCollection<string> _groupViews;
    private ObservableCollection<string> _restrictedViewNames;
    private ObservableCollection<RevitViewViewModel> _selectedViews;

    public CopyViewViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        ReplacePrefix = true;
        CopyWithDetail = true;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public bool CopyWithDetail {
        get => _copyWithDetail;
        set => RaiseAndSetIfChanged(ref _copyWithDetail, value);
    }

    public bool IsAllowReplacePrefix {
        get => _isAllowReplacePrefix;
        set => RaiseAndSetIfChanged(ref _isAllowReplacePrefix, value);
    }

    public bool IsAllowReplaceSuffix {
        get => _isAllowReplaceSuffix;
        set => RaiseAndSetIfChanged(ref _isAllowReplaceSuffix, value);
    }

    public bool ReplacePrefix {
        get => _replacePrefix;
        set => RaiseAndSetIfChanged(ref _replacePrefix, value);
    }

    public bool ReplaceSuffix {
        get => _replaceSuffix;
        set => RaiseAndSetIfChanged(ref _replaceSuffix, value);
    }

    public bool WithElevation {
        get => _withElevation;
        set => RaiseAndSetIfChanged(ref _withElevation, value);
    }

    public string GroupView {
        get => _groupView;
        set => RaiseAndSetIfChanged(ref _groupView, value);
    }

    public string Prefix {
        get => _prefix;
        set => RaiseAndSetIfChanged(ref _prefix, value);
    }

    public string Suffix {
        get => _suffix;
        set => RaiseAndSetIfChanged(ref _suffix, value);
    }

    public ObservableCollection<string> Prefixes {
        get => _prefixes;
        private set => RaiseAndSetIfChanged(ref _prefixes, value);
    }

    public ObservableCollection<string> Suffixes {
        get => _suffixes;
        private set => RaiseAndSetIfChanged(ref _suffixes, value);
    }

    public ObservableCollection<string> GroupViews {
        get => _groupViews;
        set => RaiseAndSetIfChanged(ref _groupViews, value);
    }

    public ObservableCollection<string> RestrictedViewNames {
        get => _restrictedViewNames;
        set => RaiseAndSetIfChanged(ref _restrictedViewNames, value);
    }

    public ObservableCollection<RevitViewViewModel> SelectedViews {
        get => _selectedViews;
        set => RaiseAndSetIfChanged(ref _selectedViews, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        _revitRepository.TransferStandards();

        List<View> views = _revitRepository.GetViews()
            .Where(_revitRepository.IsCopyView)
            .ToList();

        GroupViews = new ObservableCollection<string>(_revitRepository.GetGroupViews(views));
        RestrictedViewNames = new ObservableCollection<string>(_revitRepository.GetViewNames(views));

        SelectedViews = new ObservableCollection<RevitViewViewModel>(
            _revitRepository.GetSelectedCopyViews()
                .Select(item => new RevitViewViewModel(item)));

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

        string[] groupViews = SelectedViews
            .Select(item => item.GroupView)
            .Distinct()
            .ToArray();

        if(groupViews.Length == 1) {
            GroupView = groupViews.First();
        }
    }

    private void AcceptView() {
        using var transaction =
            _revitRepository.StartTransaction(_localizationService.GetLocalizedString("CopyView.TransactionName"));

        foreach(var revitView in SelectedViews) {
            var copyOption = CopyWithDetail ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;

            var newView = _revitRepository.GetElement<View>(revitView.Duplicate(copyOption));
            newView.Name = GetViewName(revitView);

            // У некоторых видов установлен шаблон,
            // у которого заблокировано редактирование атрибута ProjectParamsConfig.Instance.ViewGroup
            // удаление шаблона разрешает изменение данного атрибута
            newView.ViewTemplateId = ElementId.InvalidElementId;
            newView.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, GroupView);
        }

        transaction.Commit();
    }

    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(GroupView)) {
            ErrorText = _localizationService.GetLocalizedString("CopyView.EmptyGroupView");
            return false;
        }

        string[] generatingNames = SelectedViews
            .Select(GetViewName)
            .ToArray();

        string generateName = generatingNames.GroupBy(item => item)
            .Where(item => item.Count() > 1)
            .Select(item => item.Key)
            .FirstOrDefault();

        if(!string.IsNullOrEmpty(generateName)) {
            ErrorText = _localizationService.GetLocalizedString("CopyView.FoundRepeatedViewName", generateName);
            return false;
        }

        string existingName =
            generatingNames.FirstOrDefault(item => RestrictedViewNames.Any(item.Equals));

        if(!string.IsNullOrEmpty(existingName)) {
            ErrorText = _localizationService.GetLocalizedString("CopyView.FoundExistsViewName");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private string GetViewName(RevitViewViewModel revitView) {
        var splitViewOptions = new SplitViewOptions {
            ReplacePrefix = ReplacePrefix,
            ReplaceSuffix = ReplaceSuffix
        };

        var splittedViewName = revitView.SplitName(splitViewOptions);
        splittedViewName.Prefix = Prefix;
        splittedViewName.Suffix = Suffix;
        splittedViewName.Elevations = WithElevation ? SplittedViewName.GetElevation(revitView.View) : null;

        return Delimiter.CreateViewName(splittedViewName);
    }
}
