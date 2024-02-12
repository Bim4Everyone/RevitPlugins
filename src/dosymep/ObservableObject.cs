using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace dosymep.WPF {
    internal class ObservableObject : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue,
            [CallerMemberName] string propertyName = null) {
            if(!EqualityComparer<TRet>.Default.Equals(backingField, newValue)) {
                backingField = newValue;
                RaisePropertyChanged(propertyName);
            }
        }

        protected virtual void RaisePropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}