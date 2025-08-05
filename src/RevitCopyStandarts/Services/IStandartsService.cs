using System.Collections.Generic;
using System.IO;

namespace RevitCopyStandarts.Services;

internal interface IStandartsService {
    string GetBimPart(FileInfo bimFile);
    string GetFileName(FileInfo bimFile);
    IEnumerable<FileInfo> GetStandartsFiles();
}
