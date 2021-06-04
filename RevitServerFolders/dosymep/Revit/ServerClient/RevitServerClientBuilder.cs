using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dosymep.Revit.ServerClient {
    public class RevitServerClientBuilder {
        private string _serverName;
        private string _serverVersion;
        private ISerializer _serializer;

        public RevitServerClientBuilder SetServerName(string serverName) {
            _serverName = serverName;
            return this;
        }

        public RevitServerClientBuilder SetServerVersion(string serverVersion) {
            _serverVersion = serverVersion;
            return this;
        }

        public RevitServerClientBuilder UseJsonNetSerializer() {
            _serializer = new JsonNetSerializer();
            return this;
        }

        public IRevitServerClient Build() {
            return new RevitServerClient(_serverName, _serverVersion, _serializer);
        }
    }
}
