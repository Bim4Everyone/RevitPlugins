using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.Revit.ServerClient;
using dosymep.WPF.Commands;

namespace dosymep.Views.Revit {
    public class RevitServerViewModel {
        public RevitServerViewModel(string serverName, string serverVersion) {
            ServerName = serverName;
            ServerVersion = serverVersion;
            
            var revitClient = new RevitServerClientBuilder()
                .SetServerName(serverName)
                .SetServerVersion(serverVersion)
                .UseJsonNetSerializer()
                .Build();

            var revitServerInfo = new RevitServerInfo(revitClient);
            revitServerInfo.ExpandCommand.Execute(null);

            Root = revitServerInfo.Children;
            SetSelectedItemCommand = new RelayCommand(p => SelectedItem = (RevitServerInfo) p);
        }

        public string ServerName { get; }
        public string ServerVersion { get; }

        public ICommand SetSelectedItemCommand { get; }
        public RevitServerInfo SelectedItem { get; private set; }
        public ObservableCollection<RevitServerInfo> Root { get; private set; }
    }
}