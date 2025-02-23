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

        private readonly LinkTypeElement _linkElement;
        private string _status;
        private bool _isChecked;

        public LinkViewModel(LinkTypeElement linkElement) {

            _linkElement = linkElement;

            Name = linkElement.Name;
            FullName = linkElement.FullName;
            Status = GetStatus();
            Id = linkElement.Id;

            ReloadCommand = RelayCommand.Create(ReloadLinkDocument, CanReloadLinkDocument);
        }

        public ICommand ReloadCommand { get; set; }

        public string Status {
            get => _status;
            set => RaiseAndSetIfChanged(ref _status, value);
        }

        public bool IsChecked {
            get => _isChecked;
            set {
                if(SetAndNotifyIfChanged(ref _isChecked, value)) {
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string Name { get; }
        public string FullName { get; }
        public ElementId Id { get; }

        public event EventHandler SelectionChanged;

        private bool SetAndNotifyIfChanged<T>(ref T backingField, T newValue,
            [CallerMemberName] string propertyName = null) {
            bool changed = !EqualityComparer<T>.Default.Equals(backingField, newValue);
            RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
            return changed;
        }

        private void ReloadLinkDocument() {
            _linkElement.Reload();
            Status = GetStatus();
        }

        private bool CanReloadLinkDocument() {
            if(_isChecked) {
                return true;
            }
            return false;
        }

        private string GetStatus() {
            return $"{_linkElement.RevitLink.AttachmentType}_{_linkElement.RevitLink.GetLinkedFileStatus()}";
        }

    }
}
