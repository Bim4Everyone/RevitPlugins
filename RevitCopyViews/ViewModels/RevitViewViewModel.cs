using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class RevitViewViewModel : BaseViewModel {
        private readonly View _view;

        private string _prefix;
        private string _suffix;
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

                SplitName(Delimeter);
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

        public string Suffix {
            get => _suffix;
            set {
                _suffix = value;
                OnPropertyChanged(nameof(Suffix));
            }
        }

        private void SplitName(Delimiter delimeter) {
            int index = OriginalName.IndexOf(delimeter.Value);
            Prefix = OriginalName.Substring(0, index);

            index += delimeter.Value.Length;
            ViewName = OriginalName.Substring(index, OriginalName.Length - index);

            index = OriginalName.LastIndexOf(delimeter.Value);
            Suffix = OriginalName.Substring(index, OriginalName.Length - index);
        }

        public ElementId Duplicate(ViewDuplicateOption option) {
            return _view.Duplicate(option);
        }
    }
}
