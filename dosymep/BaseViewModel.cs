using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace dosymep.WPF.ViewModels {
    internal class BaseViewModel : dosymep.Xpf.Core.BaseViewModel {
        protected void RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue,
            [CallerMemberName] string propertyName = null) {
            if(!EqualityComparer<TRet>.Default.Equals(backingField, newValue)) {
                backingField = newValue;
                RaisePropertyChanged(propertyName);
            }
        }
    }
}