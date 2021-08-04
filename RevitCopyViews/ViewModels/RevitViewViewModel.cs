using System;
using System.Collections.Generic;
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
        }

        public string GroupView { get; }
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

        private void SplitName(Delimiter delimeter) {
            SplittedViewName splittedViewName = delimeter.SplitViewName(OriginalName, new SplitViewOptions() { ReplacePrefix = false, ReplaceSuffix = false });

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
