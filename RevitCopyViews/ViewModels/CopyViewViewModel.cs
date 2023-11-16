using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.Templates;
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
        private bool _isAllowReplaceSuffix;
        private bool _isAllowReplacePrefix;

        public CopyViewViewModel(List<View> selectedViews) {
            _selectedViews = selectedViews;

            Prefixes = new ObservableCollection<string>();
            RevitViewViewModels = new ObservableCollection<RevitViewViewModel>(_selectedViews.Select(item => new RevitViewViewModel(item)));

            ReplacePrefix = true;
            CopyWithDetail = true;
            CopyViewsCommand = new RelayCommand(CopyViews, CanCopyViews);

            Reload();
        }

        public Document Document { get; set; }
        public UIDocument UIDocument { get; set; }
        public Application Application { get; set; }

        public List<string> RestrictedViewNames { get; set; }

        public ICommand CopyViewsCommand { get; }

        public bool IsAllowReplacePrefix {
            get => _isAllowReplacePrefix;
            set => this.RaiseAndSetIfChanged(ref _isAllowReplacePrefix, value);
        }
        public bool IsAllowReplaceSuffix {
            get => _isAllowReplaceSuffix;
            set => this.RaiseAndSetIfChanged(ref _isAllowReplaceSuffix, value);
        }

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

        private void Reload() {
            Prefixes = new ObservableCollection<string>(RevitViewViewModels.Select(item => item.Prefix).Where(item => !string.IsNullOrEmpty(item)).Distinct());
            Suffixes = new ObservableCollection<string>(RevitViewViewModels.Select(item => item.Suffix).Where(item => !string.IsNullOrEmpty(item)).Distinct());

            IsAllowReplacePrefix = Prefixes.Count > 0;
            IsAllowReplaceSuffix = Suffixes.Count > 0;

            ReplacePrefix = IsAllowReplacePrefix ? ReplacePrefix : false;
            ReplaceSuffix = IsAllowReplaceSuffix ? ReplaceSuffix : false;

            if(Prefixes.Count == 1) {
                Prefix = Prefixes.First();
            }

            if(Suffixes.Count == 1) {
                Suffix = Suffixes.First();
            }

            string[] groupViews = RevitViewViewModels.Select(item => item.GroupView).Distinct().ToArray();
            if(groupViews.Length == 1) {
                GroupView = groupViews.First();
            }
        }

        private void CopyViews(object p) {
            using(var transaction = Document.StartTransaction("Копирование видов")) {
                foreach(RevitViewViewModel revitView in RevitViewViewModels) {
                    var copyOption = CopyWithDetail ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;

                    View newView = (View) Document.GetElement(revitView.Duplicate(copyOption));
                    newView.Name = GetViewName(revitView);

                    // У некоторых видов установлен шаблон,
                    // у которого заблокировано редактирование атрибута ProjectParamsConfig.Instance.ViewGroup
                    // удаление шаблона разрешает изменение данного атрибута
                    newView.ViewTemplateId = ElementId.InvalidElementId;
                    newView.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, GroupView);
                }

                transaction.Commit();
            }
        }

        private string GetViewName(RevitViewViewModel revitView) {
            var splitViewOptions = new SplitViewOptions() {
                ReplacePrefix = ReplacePrefix,
                ReplaceSuffix = ReplaceSuffix
            };

            SplittedViewName splittedViewName = revitView.SplitName(splitViewOptions);
            splittedViewName.Prefix = Prefix;
            splittedViewName.Suffix = Suffix;
            splittedViewName.Elevations = WithElevation ? SplittedViewName.GetElevation(revitView.View) : null;

            return Delimiter.CreateViewName(splittedViewName);
        }

        private bool CanCopyViews(object p) {
            if(string.IsNullOrEmpty(GroupView)) {
                ErrorText = "Не заполнена группа видов.";
                return false;
            }

            IEnumerable<string> generatingNames = RevitViewViewModels.Select(item => GetViewName(item));
            string generateName = generatingNames.GroupBy(item => item).Where(item => item.Count() > 1).Select(item => item.Key).FirstOrDefault();
            if(!string.IsNullOrEmpty(generateName)) {
                ErrorText = $"Найдено повторяющееся имя вида \"{generateName}\".";
                return false;
            }


            string existintName = generatingNames.FirstOrDefault(item => RestrictedViewNames.Any(viewName => item.Equals(viewName)));
            if(!string.IsNullOrEmpty(existintName)) {
                ErrorText = $"Найдено существующее имя вида \"{existintName}\".";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}