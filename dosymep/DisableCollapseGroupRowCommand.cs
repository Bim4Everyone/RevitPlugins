using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using DevExpress.Xpf.Grid;

namespace dosymep.WPF.Commands {
    internal class DisableCollapseGroupRowCommand : ICommand {
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning disable CS0067

        public bool CanExecute(object parameter) {
            return parameter is RowAllowEventArgs;
        }

        public void Execute(object parameter) {
            if(parameter is RowAllowEventArgs args) {
                args.Allow = false;
            }
        }
    }
}
