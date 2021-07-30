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

        public CopyViewViewModel(List<View> selectedViews) {
            _selectedViews = selectedViews;

            Prefixes = new ObservableCollection<string>();
            RevitViewViewModels = new ObservableCollection<RevitViewViewModel>(_selectedViews.Select(item => new RevitViewViewModel(item)));
        }

        public ObservableCollection<Delimiter> Delimeters { get; }
            = new ObservableCollection<Delimiter>() {
            new Delimiter() { DisplayValue = "Пробел", Value = " " },
            new Delimiter() { DisplayValue = "_", Value = "_" },
        };

        public Delimiter Delimeter {
            get => _delimeter;
            set {
                _delimeter = value;
                OnPropertyChanged(nameof(Delimeter));

                foreach(RevitViewViewModel revitView in RevitViewViewModels) {
                    revitView.Delimeter = Delimeter;
                }

                Prefixes = new ObservableCollection<string>(RevitViewViewModels.Select(item => item.Prefix).OrderBy(item => item));
            }
        }

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
    }
}