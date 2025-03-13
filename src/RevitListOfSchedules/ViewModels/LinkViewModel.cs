using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class LinkViewModel : BaseViewModel {
        public event EventHandler SelectionChanged;
        public event EventHandler StatusChanged;
        private readonly LinkTypeElement _linkElement;
        private string _status;
        private bool _isChecked;

        public LinkViewModel(LinkTypeElement linkElement) {
            _linkElement = linkElement;
            Status = GetStatus();

            ReloadCommand = RelayCommand.Create(ReloadLinkType, CanReloadLinkType);
        }
        public ICommand ReloadCommand { get; set; }
        public string Name => _linkElement.Name;
        public string FullName => _linkElement.FullName;
        public ElementId Id => _linkElement.Id;
        public bool StatusBool { get; private set; }

        public string Status {
            get => _status;
            set {
                if(SetAndNotifyIfChanged(ref _status, value)) {
                    StatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsChecked {
            get => _isChecked;
            set {
                if(SetAndNotifyIfChanged(ref _isChecked, value)) {
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool SetAndNotifyIfChanged<T>(ref T backingField, T newValue,
            [CallerMemberName] string propertyName = null) {
            bool changed = !EqualityComparer<T>.Default.Equals(backingField, newValue);
            RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
            return changed;
        }

        public void ReloadLinkType() {
            _linkElement.Reload();
            Status = GetStatus();
        }

        private bool CanReloadLinkType() {
            if(_isChecked) {
                return true;
            }
            return false;
        }

        private string GetStatus() {
            StatusBool = GetStatusBool();
            return $"{_linkElement.RevitLink.AttachmentType}_{_linkElement.RevitLink.GetLinkedFileStatus()}";

        }

        private bool GetStatusBool() {
            return _linkElement.RevitLink.GetLinkedFileStatus() is LinkedFileStatus.Loaded;
        }
    }
}
