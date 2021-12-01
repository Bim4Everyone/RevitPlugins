using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFamilyExplorer.Models {
    internal class FamilyRepository {
        private readonly string _rootFolder;

        public FamilyRepository(string rootFolder) {
            _rootFolder = rootFolder;
        }

        public Task<IEnumerable<FileInfo>> GetSections() {
            return Task.Run(GetSectionsInternal);
        }

        public Task<IEnumerable<DirectoryInfo>> GetSections(string filePath) {
            return Task.Run(() => GetSectionInternal(filePath));
        }

        public IEnumerable<FileInfo> GetSectionsInternal() {
            foreach(var filePath in Directory.EnumerateFiles(_rootFolder, "*.families")) {
                yield return new FileInfo(filePath);
            }
        }

        public IEnumerable<DirectoryInfo> GetSectionInternal(string filePath) {
            if(!File.Exists(filePath)) {
                throw new FileNotFoundException($"Не был найден файл раздела: \"{filePath}\"");
            }

            return File.ReadAllLines(filePath)
                .Select(item => new DirectoryInfo(item))
                .Where(item => item.Exists)
                .OrderBy(item => item.Name);
        }
    }
}
