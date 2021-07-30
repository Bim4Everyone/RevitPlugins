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
        private string _delimeter;
        private string _prefix;
        private string _viewName;

        public RevitViewViewModel(View view) {
            _view = view;
            GroupViews = (string) _view.GetParamValueOrDefault("_Группа Видов");
            OriginalName = _view.Name;
        }

        public string GroupViews { get; }
        public string OriginalName { get; }

        public string Delimeter {
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

        private void Reload(string delimeter) {
            int index = OriginalName.IndexOf(delimeter);
            Prefix = OriginalName.Substring(0, index);
            ViewName = OriginalName.Substring(index, OriginalName.Length);
        }
    }
}
