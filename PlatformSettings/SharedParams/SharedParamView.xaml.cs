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

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace PlatformSettings.SharedParams {
    /// <summary>
    /// Interaction logic for SharedParamView.xaml
    /// </summary>
    public partial class SharedParamView : UserControl {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RevitParamViewModel), typeof(SharedParamView));

        public SharedParamView() {
            InitializeComponent();
        }

        public RevitParamViewModel ViewModel {
            get { return (RevitParamViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }

    public class RevitParamViewModel : INotifyPropertyChanged {
        private readonly RevitParam _revitParam;

        public RevitParamViewModel(RevitParam revitParam) {
            _revitParam = revitParam;
        }
        public string Description { get => _revitParam.Description; }
        public StorageType SharedParamType { get => _revitParam.SharedParamType; }
        
        public string Name {
            get => _revitParam.Name;
            set {
                _revitParam.Name = value;
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
