using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels;

internal class RevitViewViewModel : BaseViewModel {
    private readonly SplitViewOptions _defaultSplitViewOptions = new() {
        ReplacePrefix = true,
        ReplaceSuffix = true
    };

    private string _prefix;
    private string _suffix;
    private string _viewName;

    public RevitViewViewModel(View view) {
        View = view;
        GroupView = (string) View.GetParamValueOrDefault(ProjectParamsConfig.Instance.ViewGroup);
        OriginalName = View.Name;

        if(view.ViewType
           is ViewType.FloorPlan
           or ViewType.CeilingPlan
           or ViewType.AreaPlan
           or ViewType.EngineeringPlan) {
            Elevation = view.GenLevel.Elevation;
        }

        SplitName();
    }

    public View View { get; }

    public string GroupView { get; }
    public double Elevation { get; }
    public string OriginalName { get; }

    public string Prefix {
        get => _prefix;
        private set => RaiseAndSetIfChanged(ref _prefix, value);
    }

    public string ViewName {
        get => _viewName;
        private set => RaiseAndSetIfChanged(ref _viewName, value);
    }

    public string Suffix {
        get => _suffix;
        set => RaiseAndSetIfChanged(ref _suffix, value);
    }

    public void SplitName() {
        var splittedViewName = SplitName(_defaultSplitViewOptions);

        Prefix = splittedViewName.Prefix;
        Suffix = splittedViewName.Suffix;
        ViewName = splittedViewName.ViewName;
    }

    public SplittedViewName SplitName(SplitViewOptions splitViewOptions) {
        return SplitName(OriginalName, splitViewOptions);
    }

    public SplittedViewName SplitName(string originalName, SplitViewOptions splitViewOptions) {
        return Delimiter.SplitViewName(originalName, splitViewOptions);
    }

    public ElementId Duplicate(ViewDuplicateOption option) {
        return View.Duplicate(option);
    }
}
