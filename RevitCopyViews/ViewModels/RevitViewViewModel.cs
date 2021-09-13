using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class RevitViewViewModel : BaseViewModel {
        private readonly View _view;
        private readonly SplitViewOptions _defaultSplitViewOptions = new SplitViewOptions() { ReplacePrefix = true, ReplaceSuffix = true };

        private string _prefix;
        private string _suffix;
        private string _viewName;

        public RevitViewViewModel(View view) {
            _view = view;
            GroupView = (string) _view.GetParamValueOrDefault(ProjectParamsConfig.Instance.ViewGroup);
            OriginalName = _view.Name;

            if(view.ViewType == ViewType.FloorPlan
                || view.ViewType == ViewType.CeilingPlan
                || view.ViewType == ViewType.AreaPlan
                || view.ViewType == ViewType.EngineeringPlan) {

                Elevation = view.GenLevel.Elevation;
            }
            
            SplitName();
        }

        public View View { get => _view; }
        public string GroupView { get; }
        public double Elevation { get; }
        public string OriginalName { get; }

        public string Prefix {
            get => _prefix;
            private set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string ViewName {
            get => _viewName;
            private set => this.RaiseAndSetIfChanged(ref _viewName, value);
        }

        public string Suffix {
            get => _suffix;
            set => this.RaiseAndSetIfChanged(ref _suffix, value);
        }

        public void SplitName() {
            SplittedViewName splittedViewName = SplitName(_defaultSplitViewOptions);

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
            return _view.Duplicate(option);
        }
    }
}
