using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using dosymep.Bim4Everyone.SharedParams;

namespace PlatformSettings.SharedParams {
    /// <summary>
    /// Interaction logic for SharedParamsSettingsView.xaml
    /// </summary>
    public partial class SharedParamsSettingsView : UserControl {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(SharedParamsSettingsViewModel), typeof(SharedParamsSettingsView));

        public SharedParamsSettingsView() {
            InitializeComponent();
        }

        public SharedParamsSettingsViewModel ViewModel {
            get { return (SharedParamsSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }

    public class SharedParamsSettingsViewModel : ITabSetting, INotifyPropertyChanged {
        private readonly SharedParamsConfig _sharedParamsConfig;
        private string _path;

        public SharedParamsSettingsViewModel() {
            Path = pyRevitLabs.PyRevit.PyRevitConfigs.GetConfigFile().GetValue("PlatformSettings", "SharedParamsPath");
            _sharedParamsConfig = SharedParamsConfig.Load(Path);

            Name = "Общие параметры";
            Content = new SharedParamsSettingsView() { ViewModel = this };

            SharedParams = new ObservableCollection<SharedParamViewModel>(_sharedParamsConfig.GetSharedParams().Select(item => new SharedParamViewModel(item)));
            OnPropertyChanged(nameof(SharedParams));
        }

        public string Name { get; }
        public object Content { get; }

        public ICommand OpenFile { get; }
        public ObservableCollection<SharedParamViewModel> SharedParams { get; }

        public string Path {
            get => _path;
            set {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public void SaveSettings() {
            if(!string.IsNullOrEmpty(Path)) {
                _sharedParamsConfig.Save(Path);
                pyRevitLabs.PyRevit.PyRevitConfigs.GetConfigFile().SetValue("PlatformSettings", "SharedParamsPath", Path);
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
