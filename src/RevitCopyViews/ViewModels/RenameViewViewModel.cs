using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels;

internal class RenameViewViewModel : BaseViewModel {
    private string _errorText;
    private bool _isAllowReplacePrefix;

    private bool _isAllowReplaceSuffix;
    private string _prefix;

    private ObservableCollection<string> _prefixes;
    private string _replaceNewText;
    private string _replaceOldText;

    private bool _replacePrefix;
    private bool _replaceSuffix;
    private string _suffix;
    private ObservableCollection<string> _suffixes;
    private bool _withPrefix;

    public RenameViewViewModel(List<View> selectedViews) {
        Prefixes = [];
        RevitViewViewModels =
            new ObservableCollection<RevitViewViewModel>(selectedViews.Select(item => new RevitViewViewModel(item)));

        ReplacePrefix = true;
        RenameViewCommand = new RelayCommand(RenameView, CanRenameView);

        Reload();
    }

    public Document Document { get; set; }
    public UIDocument UIDocument { get; set; }
    public Application Application { get; set; }

    public List<string> RestrictedViewNames { get; set; }

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

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ObservableCollection<RevitViewViewModel> RevitViewViewModels { get; }

    public ObservableCollection<string> Prefixes {
        get => _prefixes;
        private set => RaiseAndSetIfChanged(ref _prefixes, value);
    }

    public ObservableCollection<string> Suffixes {
        get => _suffixes;
        private set => RaiseAndSetIfChanged(ref _suffixes, value);
    }

    public ICommand RenameViewCommand { get; }

    private void RenameView(object p) {
        using var transaction = new Transaction(Document);
        transaction.Start("Переименование видов");

        foreach(var revitView in RevitViewViewModels) {
            revitView.View.Name = GetViewName(revitView);
        }

        transaction.Commit();
    }

    private bool CanRenameView(object p) {
        string[] generatingNames = RevitViewViewModels
            .Select(GetViewName)
            .ToArray();
        
        string generateName = generatingNames.GroupBy(item => item)
            .Where(item => item.Count() > 1)
            .Select(item => item.Key)
            .FirstOrDefault();
        
        if(!string.IsNullOrEmpty(generateName)) {
            ErrorText = $"Найдено повторяющееся имя вида \"{generateName}\".";
            return false;
        }

        string existingName =
            generatingNames.FirstOrDefault(item => RestrictedViewNames.Any(viewName => item.Equals(viewName)));
        
        if(!string.IsNullOrEmpty(existingName)) {
            ErrorText = $"Найдено существующее имя вида \"{existingName}\".";
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

    private void Reload() {
        Prefixes = new ObservableCollection<string>(
            RevitViewViewModels.Select(item => item.Prefix).Where(item => !string.IsNullOrEmpty(item)).Distinct());
        Suffixes = new ObservableCollection<string>(
            RevitViewViewModels.Select(item => item.Suffix).Where(item => !string.IsNullOrEmpty(item)).Distinct());

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
}
