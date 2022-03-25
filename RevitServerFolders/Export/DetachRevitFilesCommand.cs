#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep;
using dosymep.Async;
using dosymep.Revit.ServerClient;

#endregion

namespace RevitServerFolders.Export {
    public class DetachRevitFilesCommand {
        public string ServerName { get; set; }
        public string RevitVersion { get; set; }
        public bool WithSubFolders { get; set; }

        public string FolderName { get; set; }
        public string TargetFolderName { get; set; }
        public bool CleanTargetFolder { get; set; }

        public void Execute() {
            if(string.IsNullOrEmpty(RevitVersion)) {
                throw new InvalidOperationException("Перед использованием укажите версию Revit.");
            }

            if(string.IsNullOrEmpty(ServerName)) {
                throw new InvalidOperationException("Перед использованием наименование сервера Revit.");
            }

            if(string.IsNullOrEmpty(FolderName)) {
                throw new InvalidOperationException("Перед использованием укажите папку с файлами Revit.");
            }

            if(string.IsNullOrEmpty(TargetFolderName)) {
                throw new InvalidOperationException("Перед использованием укажите папку сохранения открепленных файлов Revit.");
            }

            if(CleanTargetFolder) {
                if(Directory.Exists(TargetFolderName)) {
                    foreach(var revitFile in Directory.GetFiles(TargetFolderName, "*.rvt")) {
                        File.Delete(revitFile);
                    }
                }
            }

            var client = new RevitServerClientBuilder()
                .SetServerName(ServerName)
                .SetServerVersion(RevitVersion)
                .SetJsonSerializer(new JsonNetSerializer())
                .Build();

            var directory = AsyncHelper.RunSync(() => client.GetContentsAsync(FolderName));
            List<string> revitModels = GetFileList(client, directory);

            var doubles = revitModels.GroupBy(item => Path.GetFileName(item)).Where(item => item.Count() > 1).Select(item => item.Key).ToList();
            if(doubles.Count > 0) {
                throw new Exception($"Найдены дубли: \"{string.Join(", ", doubles)}\"");
            }

            foreach(List<string> chunk in ChunkBy<string>(revitModels, 10)) {
                Task.WaitAll(chunk.Select(item => DetachDocument(item)).ToArray());
            }
        }

        private List<string> GetFileList(IRevitServerClient client, RevitContents directory) {
            if(WithSubFolders) {
                return AsyncHelper.RunSync(() => client.GetAllRevitContentsAsync(directory.Path))
                    .SelectMany(item => item.Models.Select(model => Path.Combine(item.Path, model.Name)))
                    .ToList();
            }

            return directory.Models
                .Select(item => Path.Combine(directory.Path, item.Name))
                .ToList();
        }

        private Task DetachDocument(string source) {
            return Task.Run(() => {
                string arguments = $"createLocalRvt \"{source}\" -s {ServerName} -d \"{TargetFolderName}/\" -o";
                string fileName = $@"C:\Program Files\Autodesk\Revit {RevitVersion}\RevitServerToolCommand\RevitServerTool.exe";

                Process process = Process.Start(new ProcessStartInfo() {
                    Arguments = arguments,
                    FileName = fileName,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                });

                process.WaitForExit();
            });
        }

        public static List<List<T>> ChunkBy<T>(List<T> source, int chunkSize) {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
