using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCopyViews {
    internal class RevitViewViewModel : BaseViewModel {
        private readonly View _view;

        private string _prefix;
        private string _viewName;
        private Delimiter _delimeter;

        public RevitViewViewModel(View view) {
            _view = view;
            GroupView = (string) _view.GetParamValueOrDefault("_Группа Видов");
            OriginalName = _view.Name;
        }

        public string GroupView { get; }
        public string OriginalName { get; }

        public Delimiter Delimeter {
            get => _delimeter;
            set {
                _delimeter = value;
                OnPropertyChanged(nameof(Delimeter));

                Reload(Delimeter);
            }
        }

        public string Prefix {
            get => _prefix;
            private set {
                _prefix = value;
                OnPropertyChanged(nameof(Prefix));
            }
        }

        public string ViewName {
            get => _viewName;
            private set {
                _viewName = value;
                OnPropertyChanged(nameof(ViewName));
            }
        }

        private void Reload(Delimiter delimeter) {
            int index = OriginalName.IndexOf(delimeter.Value);
            Prefix = OriginalName.Substring(0, index);

            index += delimeter.Value.Length;
            ViewName = OriginalName.Substring(index, OriginalName.Length - index);
        }

        public ElementId Duplicate(ViewDuplicateOption option) {
            return _view.Duplicate(option);
        }
    }
}
