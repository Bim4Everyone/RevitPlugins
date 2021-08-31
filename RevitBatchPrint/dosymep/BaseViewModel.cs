using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dosymep.WPF.ViewModels {
    public class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string propertyName = null) {
            if(!EqualityComparer<TRet>.Default.Equals(backingField, newValue)) {
                backingField = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
