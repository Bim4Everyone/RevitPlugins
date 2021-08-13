using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class RenameViewViewModel : BaseViewModel {
        private string _prefix;
        private string _suffix;
        private string _replaceOldText;
        private string _replaceNewText;

        private string _errorText;

        private bool _isAllowReplaceSuffix;
        private bool _isAllowReplacePrefix;

        private bool _replacePrefix;
        private bool _replaceSuffix;

        private ObservableCollection<string> _prefixes;
        private ObservableCollection<string> _suffixes;
        private bool _withPrefix;

        public RenameViewViewModel(List<View> selectedViews) {
            Prefixes = new ObservableCollection<string>();
            RevitViewViewModels = new ObservableCollection<RevitViewViewModel>(selectedViews.Select(item => new RevitViewViewModel(item)));

            ReplacePrefix = true;
            RenameViewCommand = new RelayCommand(RenameView, CanRenameView);

            Reload();
        }

        public Document Document { get; set; }
        public UIDocument UIDocument { get; set; }
        public Application Application { get; set; }

        public List<string> RestrictedViewNames { get; set; }

        public string Prefix {
            get => _prefix;
            set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string Suffix {
            get => _suffix;
            set => this.RaiseAndSetIfChanged(ref _suffix, value);
        }

        public bool IsAllowReplacePrefix {
            get => _isAllowReplacePrefix;
            set => this.RaiseAndSetIfChanged(ref _isAllowReplacePrefix, value);
        }
        public bool IsAllowReplaceSuffix {
            get => _isAllowReplaceSuffix;
            set => this.RaiseAndSetIfChanged(ref _isAllowReplaceSuffix, value);
        }

        public bool WithPrefix { 
            get => _withPrefix; 
            set => this.RaiseAndSetIfChanged(ref _withPrefix, value); 
        }

        public bool ReplacePrefix {
            get => _replacePrefix;
            set => this.RaiseAndSetIfChanged(ref _replacePrefix, value);
        }

        public bool ReplaceSuffix {
            get => _replaceSuffix;
            set => this.RaiseAndSetIfChanged(ref _replaceSuffix, value);
        }

        public string ReplaceOldText {
            get => _replaceOldText;
            set => this.RaiseAndSetIfChanged(ref _replaceOldText, value);
        }

        public string ReplaceNewText {
            get => _replaceNewText;
            set => this.RaiseAndSetIfChanged(ref _replaceNewText, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<RevitViewViewModel> RevitViewViewModels { get; }

        public ObservableCollection<string> Prefixes {
            get => _prefixes;
            private set => this.RaiseAndSetIfChanged(ref _prefixes, value);
        }

        public ObservableCollection<string> Suffixes {
            get => _suffixes;
            private set => this.RaiseAndSetIfChanged(ref _suffixes, value);
        }

        public ICommand RenameViewCommand { get; }

        private void RenameView(object p) {
            using(var transaction = new Transaction(Document)) {
                transaction.Start("Переименование видов");

                foreach(RevitViewViewModel revitView in RevitViewViewModels) {
                    revitView.View.Name = GetViewName(revitView);
                }

                transaction.Commit();
            }
        }

        private bool CanRenameView(object p) {
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

        private string GetViewName(RevitViewViewModel revitView) {
            string originalName = revitView.OriginalName;
            if(!string.IsNullOrEmpty(ReplaceOldText)) {
                originalName = revitView.OriginalName.Replace(ReplaceOldText, ReplaceNewText);
            }

            if(!WithPrefix) {
                return originalName;
            }

            var splitViewOptions = new SplitViewOptions() {
                ReplacePrefix = ReplacePrefix,
                ReplaceSuffix = ReplaceSuffix
            };

            SplittedViewName splittedViewName = revitView.SplitName(originalName, splitViewOptions);
            splittedViewName.Prefix = Prefix;
            splittedViewName.Suffix = Suffix;

            return Delimiter.CreateViewName(splittedViewName);
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
        }
    }
}
