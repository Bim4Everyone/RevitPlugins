using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace dosymep.Revit.ServerClient {
    public class RevitServerClient {
        private readonly string _baseUrl;

        public RevitServerClient(string serverName, string revitVersion) {
            if(string.IsNullOrEmpty(serverName)) {
                throw new ArgumentException($"'{nameof(serverName)}' cannot be null or empty.", nameof(serverName));
            }

            if(string.IsNullOrEmpty(revitVersion)) {
                throw new ArgumentException($"'{nameof(revitVersion)}' cannot be null or empty.", nameof(revitVersion));
            }

            _baseUrl = $"http://{serverName}/RevitServerAdminRESTService{revitVersion}/AdminRESTService.svc";
        }

        public RevitDirectory GetRootDirectory() {
            var directory = Get<RevitDirectory>(GetRestRequest($"/|/contents"));
            directory.Models.ForEach(item => item.Directory = directory);
            directory.Folders.ForEach(item => item.Directory = directory);

            return directory;
        }

        public RevitDirectory GetDirectory(string path) {
            if(string.IsNullOrEmpty(path)) {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            var directory = Get<RevitDirectory>(GetRestRequest($"/{path.Replace('\\', '|').Replace('/', '|')}/contents"));
            directory.Models.ForEach(item => item.Directory = directory);
            directory.Folders.ForEach(item => item.Directory = directory);

            return directory;
        }

        public List<RevitFile> GetRecursiveRevitFiles(RevitDirectory directory) {
            if(directory == null) {
                return new List<RevitFile>();
            }

            if(directory.Folders.Count == 0) {
                return directory.Models;
            }

            return directory.Folders
                .SelectMany(item => GetRecursiveRevitFiles(GetDirectory(directory.Path.Replace('\\', '|') + "|" + item.Name)))
                .ToList();
        }

        private T Get<T>(WebRequest request) {
            return JsonSerializer.Deserialize<T>(new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd());
        }

        private WebRequest GetRestRequest(string resource) {
            WebRequest request = WebRequest.Create(_baseUrl + resource);
            request.Method = "GET";
            request.Headers.Add("User-Name", Environment.UserName);
            request.Headers.Add("User-Machine-Name", Environment.MachineName);
            request.Headers.Add("Operation-GUID", Guid.NewGuid().ToString());

            return request;
        }
    }
}
