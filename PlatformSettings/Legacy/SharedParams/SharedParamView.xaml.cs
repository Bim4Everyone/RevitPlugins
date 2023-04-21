using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace PlatformSettings.Legacy.SharedParams {
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
        public StorageType SharedParamType { get => _revitParam.StorageType; }
        
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
