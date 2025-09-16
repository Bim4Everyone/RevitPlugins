using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
internal class ErrorsService : IErrorsService {
    private readonly List<ErrorModel> _errors;


    public ErrorsService() {
        _errors = [];
    }


    public void AddError(ErrorModel error) {
        _errors.Add(error);
    }

    public void AddError(string modelPath, string errorDescription, ExportSettings exportSettings) {
        if(string.IsNullOrWhiteSpace(modelPath)) {
            throw new ArgumentException(nameof(modelPath));
        }
        if(string.IsNullOrWhiteSpace(errorDescription)) {
            throw new ArgumentException(nameof(errorDescription));
        }
        if(exportSettings is null) {
            throw new ArgumentNullException(nameof(exportSettings));
        }
        AddError(new ErrorModel(modelPath, errorDescription, exportSettings));
    }

    public bool ContainsErrors() {
        return _errors.Count > 0;
    }

    public ICollection<ErrorModel> GetAllErrors() {
        return new ReadOnlyCollection<ErrorModel>(_errors);
    }
}
