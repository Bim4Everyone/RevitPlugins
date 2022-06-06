using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;

using pyRevitLabs.NLog;

namespace PlatformSettings.SharedParams {
    /// <summary>
    /// Interaction logic for SharedParamsSettingsView.xaml
    /// </summary>
    public partial class SharedParamsSettingsView : UserControl {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RevitParamsSettingsViewModel), typeof(SharedParamsSettingsView));

        public SharedParamsSettingsView() {
            InitializeComponent();
        }

        public RevitParamsSettingsViewModel ViewModel {
            get { return (RevitParamsSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ViewModel.OpenFile.Execute(null);
        }
    }

    public class RevitParamsSettingsViewModel : ITabSetting, INotifyPropertyChanged {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string _path;
        private readonly RevitParams _revitParams;
        private readonly RevitParamsConfig _revitParamsConfig;

        public RevitParamsSettingsViewModel(RevitParams revitParams) {
            _path = revitParams.GetConfigPath();
            _revitParams = revitParams;
            _revitParamsConfig = revitParams.GetConfig();

            Name = revitParams.Name;
            Content = new SharedParamsSettingsView() { ViewModel = this };

            SharedParams = new ObservableCollection<RevitParamViewModel>(_revitParamsConfig.GetRevitParams().Select(item => new RevitParamViewModel(item)));
            OnPropertyChanged(nameof(SharedParams));

            OpenFile = new RelayCommand(SelectConfigFile);
            this._revitParams = revitParams;
        }

        public string Name { get; }
        public object Content { get; }

        public ICommand OpenFile { get; }
        public ObservableCollection<RevitParamViewModel> SharedParams { get; }

        public bool IsAllowEditParams {
            get => !string.IsNullOrEmpty(Path);
        }

        public string Path {
            get => _path;
            set {
                _path = value;
                OnPropertyChanged(nameof(Path));
                OnPropertyChanged(nameof(IsAllowEditParams));
            }
        }

        public void SaveSettings() {
            try {
                if(!string.IsNullOrEmpty(Path)) {
                    _revitParams.SaveSettings(_revitParamsConfig, Path);
                }
            } catch(Exception ex) {
                logger.Error(ex, "Ошибка сохранения конфигурации параметров.");
            }
        }

        private void SelectConfigFile(object p) {
            using(var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.SelectedPath = System.IO.Path.GetDirectoryName(Path);
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Path = System.IO.Path.Combine(dialog.SelectedPath, _revitParams.ConfigFileName);
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
