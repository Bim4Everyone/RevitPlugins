using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class CopyViewViewModel : BaseViewModel {
        private List<View> _selectedViews;

        private string _prefix;
        private string _suffix;
        private string _groupView;
        private string _errorText;

        private ObservableCollection<string> _prefixes;
        private ObservableCollection<string> _suffixes;
        private ObservableCollection<string> _groupViews;

        private bool _copyWithDetail;
        private bool _replacePrefix;
        private bool _replaceSuffix;
        private bool _withElevation;

        public CopyViewViewModel(List<View> selectedViews) {
            _selectedViews = selectedViews;

            Prefixes = new ObservableCollection<string>();
            RevitViewViewModels = new ObservableCollection<RevitViewViewModel>(_selectedViews.Select(item => new RevitViewViewModel(item)));

            ReplacePrefix = true;
            CopyWithDetail = true;
            CopyViewsCommand = new RelayCommand(CopyViews, CanCopyViews);
        }

        public Document Document { get; set; }
        public Application Application { get; set; }

        public ICommand CopyViewsCommand { get; }

        public bool ReplacePrefix {
            get => _replacePrefix;
            set => this.RaiseAndSetIfChanged(ref _replacePrefix, value);
        }

        public bool ReplaceSuffix {
            get => _replaceSuffix;
            set => this.RaiseAndSetIfChanged(ref _replaceSuffix, value);
        }

        public bool WithElevation { 
            get => _withElevation;
            set => this.RaiseAndSetIfChanged(ref _withElevation, value);
        }

        public bool CopyWithDetail {
            get => _copyWithDetail;
            set => this.RaiseAndSetIfChanged(ref _copyWithDetail, value);
        }

        public string Prefix {
            get => _prefix;
            set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public ObservableCollection<string> Prefixes {
            get => _prefixes;
            private set => this.RaiseAndSetIfChanged(ref _prefixes, value);
        }

        public string Suffix {
            get => _suffix;
            set => this.RaiseAndSetIfChanged(ref _suffix, value);
        }

        public ObservableCollection<string> Suffixes {
            get => _suffixes;
            private set => this.RaiseAndSetIfChanged(ref _suffixes, value);
        }

        public string GroupView {
            get => _groupView;
            set => this.RaiseAndSetIfChanged(ref _groupView, value);
        }

        public ObservableCollection<string> GroupViews {
            get => _groupViews;
            set => this.RaiseAndSetIfChanged(ref _groupViews, value);
        }

        public ObservableCollection<RevitViewViewModel> RevitViewViewModels { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private void CopyViews(object p) {
            using(var transaction = new Transaction(Document)) {
                transaction.Start("Копирование видов");

                foreach(RevitViewViewModel revitView in RevitViewViewModels) {
                    var option = CopyWithDetail ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;

                    View newView = (View) Document.GetElement(revitView.Duplicate(option));
                    SplittedViewName splittedViewName = revitView.SplitName(new SplitViewOptions() { ReplacePrefix = ReplacePrefix, ReplaceSuffix = ReplaceSuffix });

                    var list = new List<string>();
                    if(!string.IsNullOrEmpty(Prefix)) {
                        list.Add(Prefix);
                    }

                    list.Add(splittedViewName.ViewName);

                    if(WithElevation && !string.IsNullOrEmpty(revitView.Elevation)) {
                        list.Add(revitView.Elevation);
                    }

                    if(!string.IsNullOrEmpty(Suffix)) {
                        list.Add(Suffix);
                    }

                    newView.Name = string.Join(Delimiter.Value, list);

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
}