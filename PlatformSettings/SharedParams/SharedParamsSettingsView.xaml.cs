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

using pyRevitLabs.NLog;

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

        private void Button_Click(object sender, RoutedEventArgs e) {
            ViewModel.OpenFile.Execute(null);
        }
    }

    public class SharedParamsSettingsViewModel : ITabSetting, INotifyPropertyChanged {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string _path;
        private readonly SharedParamsConfig _sharedParamsConfig;

        public SharedParamsSettingsViewModel() {
            string sharedParamsPath = pyRevitLabs.PyRevit.PyRevitConfigs.GetConfigFile().GetValue("PlatformSettings", "SharedParamsPath");
            if(!string.IsNullOrEmpty(sharedParamsPath)) {
                Path = new System.IO.FileInfo(sharedParamsPath.Trim('\"')).FullName;
            }

            _sharedParamsConfig = SharedParamsConfig.Load(Path);

            Name = "Общие параметры";
            Content = new SharedParamsSettingsView() { ViewModel = this };

            SharedParams = new ObservableCollection<SharedParamViewModel>(_sharedParamsConfig.GetSharedParams().Select(item => new SharedParamViewModel(item)));
            OnPropertyChanged(nameof(SharedParams));

            OpenFile = new RelayCommand(SelectConfigFile);
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
            try {
                if(!string.IsNullOrEmpty(Path)) {
                    _sharedParamsConfig.Save(Path);
                    pyRevitLabs.PyRevit.PyRevitConfigs.GetConfigFile().SetValue("PlatformSettings", "SharedParamsPath", $"\"{Path}\"");
                }
            } catch(Exception ex) {
                logger.Error(ex, "Ошибка сохранения конфигурации общих параметров.");
            }
        }

        private void SelectConfigFile(object p) {
            using(var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.SelectedPath = System.IO.Path.GetDirectoryName(Path);
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Path = System.IO.Path.Combine(dialog.SelectedPath, "shared_params.json");
                }
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
