using System;
using System.Globalization;
using System.IO;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

namespace RevitServerFolders.Utils {
    internal static class Extensions {
        private const int Size = 1024;
        private static readonly string[] _units = {"б", "Кб", "Мб", "Гб", "Тб", "Пб", "Еб"};
        
        public static string BytesToString(long size) {
            if(size == 0) {
                return "0 " + _units[0];
            }

            long bytes = Math.Abs(size);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, Size)));
            double num = Math.Round(bytes / Math.Pow(Size, place), 1);
            return (Math.Sign(size) * num).ToString(CultureInfo.CurrentCulture) + " " + _units[place];
        }

        /// <summary>Returns visible model path for RS.</summary>
        /// <param name="serverClient">Server client connection.</param>
        /// <param name="folderContents">Parent folder contents.</param>
        /// <param name="objectData">Object data.</param>
        /// <returns>Returns visible model path for RS.</returns>
        public static string GetVisibleModelPath(this IServerClient serverClient,
            FolderContents folderContents,
            ObjectData objectData) {
            return new UriBuilder("RSN",
                serverClient.ServerName,
                -1,
                folderContents.GetRelativeModelPath(objectData)).Uri.ToString();
        }
    }
}
