using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;

namespace RevitCopyViews {
    internal class CopyViewViewModel : BaseViewModel {
        private Delimiter _delimeter;
        private List<View> _selectedViews;

        private string _prefix;
        private string _suffix;
        private string _groupView;
        private string _errorText;

        private ObservableCollection<string> _prefixes;
        private ObservableCollection<string> _groupViews;
        private bool _copyWithDetail;

        public CopyViewViewModel(List<View> selectedViews) {
            _selectedViews = selectedViews;

            Prefixes = new ObservableCollection<string>();
            RevitViewViewModels = new ObservableCollection<RevitViewViewModel>(_selectedViews.Select(item => new RevitViewViewModel(item)));

            Delimeters = new ObservableCollection<Delimiter>() {
                    new Delimiter() { DisplayValue = "_", Value = "_" },
                    new Delimiter() { DisplayValue = "Пробел", Value = " " },
            };

            Delimeter = Delimeters.FirstOrDefault();

            CopyWithDetail = true;
            CopyViewsCommand = new RelayCommand(CopyViews, CanCopyViews);
        }

        public Document Document { get; set; }
        public Application Application { get; set; }

        public ICommand CopyViewsCommand { get; }

        public bool CopyWithDetail {
            get => _copyWithDetail;
            set {
                _copyWithDetail = value;
                OnPropertyChanged(nameof(CopyWithDetail));
            }
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


        public string ErrorText {
            get => _errorText;
            set {
                _errorText = value;
                OnPropertyChanged(nameof(ErrorText));
            }
        }

        private void CopyViews(object p) {
            using(var transaction = new Transaction(Document)) {
                transaction.Start("Копирование видов");

                foreach(RevitViewViewModel revitView in RevitViewViewModels) {
                    var option = CopyWithDetail ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;
                    
                    View newView = (View) Document.GetElement(revitView.Duplicate(option));
                    newView.Name = string.Join(Delimeter.Value, Prefix, revitView.ViewName, Suffix);

                    // У некоторых видов установлен шаблон,
                    // у которого заблокировано редактирование атрибута "_Группа Видов"
                    // удаление шаблона разрешает изменение данного атрибута
                    newView.ViewTemplateId = ElementId.InvalidElementId;
                    newView.SetParamValue("_Группа Видов", GroupView);
                }

                transaction.Commit();
            }
        }

        private bool CanCopyViews(object p) {
            if(Delimeter == null) {
                ErrorText = "Не выбран разделитель для префикса.";
                return false;
            }

            if(string.IsNullOrEmpty(Prefix)) {
                ErrorText = "Не заполнен префикс.";
                return false;
            }

            if(string.IsNullOrEmpty(GroupView)) {
                ErrorText = "Не заполнена группа видов.";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }

    internal class Delimiter {
        public string Value { get; set; }
        public string DisplayValue { get; set; }

        public override string ToString() {
            return DisplayValue;
        }
    }
}