using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyViews {
    internal class CopyViewViewModel : BaseViewModel {
        public List<View> _selectedViews;
        private Delimiter _delimeter;
        private ObservableCollection<string> _prefixes;
        private string _prefix;
        private string _suffix;
        private string _groupView;
        private ObservableCollection<string> _groupViews;

        public CopyViewViewModel(List<View> selectedViews) {
            _selectedViews = selectedViews;


            Prefixes = new ObservableCollection<string>();
            RevitViewViewModels = new ObservableCollection<RevitViewViewModel>(_selectedViews.Select(item => new RevitViewViewModel(item)));

            Delimeters = new ObservableCollection<Delimiter>() {
                    new Delimiter() { DisplayValue = "_", Value = "_" },
                    new Delimiter() { DisplayValue = "Пробел", Value = " " },
            };

            Delimeter = Delimeters.FirstOrDefault();
        }

        public string Prefix {
            get => _prefix;
            set {
                _prefix = value;
                OnPropertyChanged(nameof(Prefix));
            }
        }

        public string Suffix {
            get => _suffix;
            set {
                _suffix = value;
                OnPropertyChanged(nameof(Suffix));
            }
        }

        public string GroupView {
            get => _groupView;
            set {
                _groupView = value;
                OnPropertyChanged(nameof(GroupView));
            }
        }

        public ObservableCollection<string> GroupViews {
            get => _groupViews;
            set {
                _groupViews = value;
                OnPropertyChanged(nameof(GroupViews));
            }
        }

        public Delimiter Delimeter {
            get => _delimeter;
            set {
                _delimeter = value;
                OnPropertyChanged(nameof(Delimeter));

                foreach(RevitViewViewModel revitView in RevitViewViewModels) {
                    revitView.Delimeter = Delimeter;
                }

                Prefixes = new ObservableCollection<string>(RevitViewViewModels.Select(item => item.Prefix).Distinct().OrderBy(item => item));
                Prefix = Prefixes.FirstOrDefault();
            }
        }

        public ObservableCollection<Delimiter> Delimeters { get; }

        public ObservableCollection<string> Prefixes {
            get => _prefixes;
            private set {
                _prefixes = value;
                OnPropertyChanged(nameof(Prefixes));
            }
        }

        public ObservableCollection<RevitViewViewModel> RevitViewViewModels { get; }
    }

    internal class Delimiter {
        public string Value { get; set; }
        public string DisplayValue { get; set; }

        public override string ToString() {
            return DisplayValue;
        }
    }
}