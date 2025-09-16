using System.Collections.Generic;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
internal interface IErrorsService {
    ICollection<ErrorModel> GetAllErrors();

    void AddError(ErrorModel error);

    void AddError(string modelPath, string errorDescription, ExportSettings exportSettings);

    bool ContainsErrors();
}
