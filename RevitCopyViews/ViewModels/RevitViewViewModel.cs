using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using ReactiveUI;

namespace RevitCopyViews.ViewModels {
    internal class RevitViewViewModel : BaseViewModel {
        private readonly View _view;
        private readonly SplitViewOptions _defaultSplitViewOptions = new SplitViewOptions() { ReplacePrefix = false, ReplaceSuffix = false };

        private string _prefix;
        private string _suffix;
        private string _elevations;
        private string _viewName;
        private Delimiter _delimeter;

        public RevitViewViewModel(View view) {
            _view = view;
            GroupView = (string) _view.GetParamValueOrDefault("_Группа Видов");
            OriginalName = _view.Name;

            this.WhenAnyValue(x => x.Delimeter)
                .WhereNotNull()
                .Subscribe(x => SplitName(x));

            if(view.ViewType == ViewType.FloorPlan
                || view.ViewType == ViewType.CeilingPlan
                || view.ViewType == ViewType.AreaPlan
                || view.ViewType == ViewType.EngineeringPlan) {

                Elevation = view.GenLevel.Elevation.ToString("0.###", CultureInfo.InvariantCulture);
            }
        }
        
        public string GroupView { get; }
        public string Elevation { get; }
        public string OriginalName { get; }

        public Delimiter Delimeter {
            get => _delimeter;
            set => this.RaiseAndSetIfChanged(ref _delimeter, value);
        }

        public string Prefix {
            get => _prefix;
            private set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string ViewName {
            get => _viewName;
            private set => this.RaiseAndSetIfChanged(ref _viewName, value);
        }

        public string Elevations {
            get => _elevations;
            set => this.RaiseAndSetIfChanged(ref _elevations, value);
        }

        public string Suffix {
            get => _suffix;
            set => this.RaiseAndSetIfChanged(ref _suffix, value);
        }

        public SplittedViewName SplitName(Delimiter delimeter, SplitViewOptions splitViewOptions) {
            return delimeter.SplitViewName(OriginalName, splitViewOptions);
        }

        private void SplitName(Delimiter delimeter) {
            SplittedViewName splittedViewName = SplitName(delimeter, _defaultSplitViewOptions);

            Prefix = splittedViewName.Prefix;
            ViewName = splittedViewName.ViewName;
            Elevations = splittedViewName.Elevations;
            Suffix = splittedViewName.Suffix;
        }

        public ElementId Duplicate(ViewDuplicateOption option) {
            return _view.Duplicate(option);
        }
    }
}
