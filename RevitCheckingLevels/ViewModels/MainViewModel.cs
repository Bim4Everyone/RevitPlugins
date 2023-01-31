using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Services;
using RevitCheckingLevels.Views;

namespace RevitCheckingLevels.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly Func<Type, Window> _modeFactory;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _isSelectCheckingLevel;
        private LinkTypeViewModel _linkType;

        public MainViewModel(RevitRepository revitRepository, Func<Type, Window> modeFactory) {
            _modeFactory = modeFactory;
            _revitRepository = revitRepository;

            IsSelectCheckingLevel = true;

            ViewCommand = new RelayCommand(ChangeMode, CanChangeMode);
            ViewLoadCommand = new RelayCommand(ViewLoad);
        }

        public ICommand ViewCommand { get; }
        public ICommand ViewLoadCommand { get; }

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; }
            = new ObservableCollection<LinkTypeViewModel>();

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool IsSelectCheckingLevel {
            get => _isSelectCheckingLevel;
            set => this.RaiseAndSetIfChanged(ref _isSelectCheckingLevel, value);
        }

        private void ViewLoad(object p) {
            LinkTypes.Clear();
            foreach(RevitLinkType linkType in _revitRepository.GetRevitLinkTypes()) {
                LinkTypes.Add(new LinkTypeViewModel(linkType));
            }

            LinkType = LinkTypes.FirstOrDefault();
        }

        private void ChangeMode(object p) {
            if(IsSelectCheckingLevel) {
                _modeFactory(typeof(CheckingLevelsWindow)).ShowDialog();
            } else {
                // _modeFactory(typeof(CheckingLinkLevelsWindow)).ShowDialog();
            }
        }

        private bool CanChangeMode(object p) {
            if(!IsSelectCheckingLevel) {
                if(LinkType == null) {
                    ErrorText = "Выберите координационный файл.";
                    return false;
                }

                if(!LinkType.IsLinkLoaded) {
                    ErrorText = "Загрузите координационный файл.";
                    return false;
                }
            }

            ErrorText = null;
            return true;
        }
    }
}