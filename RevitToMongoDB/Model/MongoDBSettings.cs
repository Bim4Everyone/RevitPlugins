using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using MongoDB.Driver;

namespace RevitToMongoDB.Model {
    public class MongoDBSettings {
        public string ProjectName { get; set; }
        public string VersionName { get; set; }
        private string _connectionString { get; set; }

        public MongoDBSettings(string connectionString, Autodesk.Revit.DB.Document document) {
            _connectionString = connectionString;
            ProjectName = document.Title;
            VersionName = GetVersionName();
        }

        public static string GetNextVersion(List<string> allVersions) {
            Regex regex = new Regex(@"v(\d+)");
            var filteredVersions = allVersions
                .Select(v => regex.Match(v))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .OrderBy(v => v);
            int nextVersionNumber = filteredVersions.Any() ? filteredVersions.Max() + 1 : 1;
            return $"v{nextVersionNumber}";
        }

        public string GetVersionName() {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase(ProjectName);
            var names = database.ListCollectionNames().ToList();
            return $"{GetNextVersion(names)} - {DateTime.Now.ToString("dd.MM.yy")}";
        }
    }
}
