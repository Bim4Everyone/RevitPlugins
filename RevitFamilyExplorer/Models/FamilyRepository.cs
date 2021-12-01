using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFamilyExplorer.Models {
    internal class FamilyRepository {
        private readonly string[] _sort;
        private readonly string _rootFolder;

        public FamilyRepository(string rootFolder) {
            _rootFolder = rootFolder;

            string sortFile = Path.Combine(_rootFolder, "sort.txt");
            _sort = File.Exists(sortFile) ? File.ReadAllLines(Path.Combine(_rootFolder, "sort.txt")) : new string[0];
        }

        public Task<IEnumerable<FileInfo>> GetSections() {
            return Task.Run(GetSectionsInternal);
        }

        public Task<IEnumerable<DirectoryInfo>> GetSections(string filePath) {
            return Task.Run(() => GetSectionInternal(filePath));
        }

        public IEnumerable<FileInfo> GetSectionsInternal() {
            if(Directory.Exists(_rootFolder)) {
                foreach(var filePath in Directory.EnumerateFiles(_rootFolder, "*.families")
                    .OrderBy(item => GetIndex(item))
                    .ThenBy(item => item)) {
                    yield return new FileInfo(filePath);
                }
            }
        }

        public IEnumerable<DirectoryInfo> GetSectionInternal(string filePath) {
            if(!File.Exists(filePath)) {
                return Enumerable.Empty<DirectoryInfo>();
            }

            return File.ReadAllLines(filePath)
                .Select(item => new DirectoryInfo(item))
                .Where(item => item.Exists)
                .OrderBy(item => item.Name);
        }

        private int GetIndex(string filePath) {
            return Array.IndexOf(_sort, Path.GetFileName(filePath));
        }
    }
}
