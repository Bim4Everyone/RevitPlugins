using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;

namespace PlatformSettings.SharedParams {
    /// <summary>
    /// Interaction logic for SharedParamView.xaml
    /// </summary>
    public partial class SharedParamView : UserControl {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(SharedParamViewModel), typeof(SharedParamView));

        public SharedParamView() {
            InitializeComponent();
        }

        public SharedParamViewModel ViewModel {
            get { return (SharedParamViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }

    public class SharedParamViewModel : INotifyPropertyChanged {
        private readonly SharedParam _sharedParam;

        public SharedParamViewModel(SharedParam sharedParam) {
            _sharedParam = sharedParam;
        }
        public string Description { get => _sharedParam.Description; }
        public StorageType SharedParamType { get => _sharedParam.SharedParamType; }
        
        public string Name {
            get => _sharedParam.Name;
            set {
                _sharedParam.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
