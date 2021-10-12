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

        public IEnumerable<DirectoryInfo> GetAR() {
            return GetSection("AR.txt");
        }

        public IEnumerable<DirectoryInfo> GetKR() {
            return GetSection("KR.txt");
        }

        public IEnumerable<DirectoryInfo> GetOV() {
            return GetSection("OV.txt");
        }

        public IEnumerable<DirectoryInfo> GetVK() {
            return GetSection("VK.txt");
        }

        public IEnumerable<DirectoryInfo> GetSS() {
            return GetSection("SS.txt");
        }

        private IEnumerable<DirectoryInfo> GetSection(string fileName) {
            string filePath = Path.Combine(_rootFolder, fileName);
            if(!File.Exists(filePath)) {
                throw new FileNotFoundException($"Не был найден файл раздела: \"{fileName}\"");
            }

            return File.ReadAllLines(filePath)
                .Select(item => new DirectoryInfo(item))
                .Where(item => item.Exists)
                .OrderBy(item => item.Name);
        }
    }
}
