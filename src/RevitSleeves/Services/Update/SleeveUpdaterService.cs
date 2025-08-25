using System;
using System.Collections.Generic;
using System.Threading;

using RevitSleeves.Models;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Update;
internal class SleeveUpdaterService : ISleeveUpdaterService {
    private readonly RevitRepository _repository;
    private readonly IErrorsService _errorsService;

    public SleeveUpdaterService(
        RevitRepository repository,
        IErrorsService errorsService) {

        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _errorsService = errorsService
            ?? throw new ArgumentNullException(nameof(errorsService));
    }


    public ICollection<SleeveModel> UpdateSleeves(
        ICollection<SleeveModel> sleeves,
        IProgress<int> progress,
        CancellationToken ct) {
        throw new NotImplementedException();
    }
}
